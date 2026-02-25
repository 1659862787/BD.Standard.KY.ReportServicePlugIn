
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

namespace BD.Standard.KY.ReportServicePlugIn
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("库存账龄报表二开报表服务插件")]
    public class ReportPlugInTwo : SysReportBaseService
    {
        //当天入库的账龄按1天计算
        private int CurDateIsOne;
        //private string FIntervalTxt = "";
        private string FQueryDate = "";
        //显示标题
        //private string F_UJED_StockOrg = "";
        private string F_UJED_MaterialRange = "全部";
        private string F_UJED_StockRange = "全部";
        private string F_UJED_LotRange = "全部";
        //private string F_UJED_StockOrgWhere = "";
        private string F_UJED_MaterialWhere = "";
        private string F_UJED_StockWhere = "";
        private string F_UJED_LotWhere = "";
        private Boolean Fisrework = true;
        private Boolean flag = true;
        private List<string> FMoBillno = new List<string>();

        private static readonly string Logpath = @"D:\KYLog\reportLog\" + DateTime.Now.ToString("yyyyMM");

        #region 设置报表
        public override void Initialize()
        {
            base.Initialize();
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            this.ReportProperty.ReportName = new LocaleValue(
                "库存账龄报表二开",
                base.Context.UserLocale.LCID
            );
            this.IsCreateTempTableByPlugin = true;
            this.ReportProperty.IsGroupSummary = true;
            this.ReportProperty.IsUIDesignerColumns = false;
            this.ReportProperty.SimpleAllCols = false;
            //this.ReportProperty.IdentityFieldName = "FIDENTITYID";
            this.SetDecimalControl();
        }
        #endregion 设置报表

        #region 汇总
        public override List<SummaryField> GetSummaryColumnInfo(IRptParams filter)
        {
            var result = base.GetSummaryColumnInfo(filter);
            result.Add(new SummaryField("Fsumfbaseqty", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("Fsumfqty", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("Fsumfamount", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            for (int i = 0; i<Field.Length; i++)
            {

                if (Field[i].Contains("IntervalTxt"))
                {
                    result.Add(new SummaryField(Field[i], Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
                }

            }


            return result;
        }
        #endregion

        private void SetDecimalControl()
        {
            List<DecimalControlField> list = new List<DecimalControlField>();

            this.ReportProperty.DecimalControlFieldList = list;
         
        }

        private Dictionary<string, string> keyValues = null;

        string[] Field = { "FMATERIALID1", "FMATERIALID", "FMATERIALNUMBER", "FStockOrgId", "FMaterialTypeName", "FSTOCKID", "FSTOCKSTATUSID", "FStockLocId", "FSTOCKLOC", "FLOTNO","FOwnerType", "FOwner", "FKeeperType", "FKeeper", "FBASEUNITID", "FUnitID", "Fbilldatemin", "FDayDiff", "Fsumfbaseqty", "Fsumfqty", "Fsumfamount" };
        string[] FieldType = { "numeric", "numeric", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "DECIMAL(23,10)", "DECIMAL(23,10)", "DECIMAL(23,10)" };


        #region 创建临时表
        public string CreateMainTempSql(string tmpTableName)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(string.Format("/*dialect*/create table {0}", tmpTableName));
            builder.AppendLine(" ( ");

            for (int i = 0; i < Field.Length; i++)
            {
                if (i == Field.Length - 1)
                {
                    builder.AppendLine(Field[i] + " " + FieldType[i] + ")");
                }
                else
                {
                    builder.AppendLine(Field[i] + " " + FieldType[i] + ",");
                }
            }
            return builder.ToString();
        }
        #endregion 创建临时表

        #region  构建报表详细信息
        public void InventoryDetailSummarySql(string temp, IRptParams filter)
        {
            string midtable = " KY_GetReportDataTable ";
            Logger logger1 = new Logger(Logpath, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
            logger1.WriteLog("是否返工单：" + Fisrework.ToString());
            if (Fisrework)
            {
                #region 返工入库上查
                StringBuilder updateBuilder = new StringBuilder();
                updateBuilder.AppendLine("/*dialect*/");
                updateBuilder.AppendLine($" select FINSTOCKBILLNO  from  KY_GetReportDataTable where FINSTOCKFORMID='PRD_INSTOCK'");
                DynamicObjectCollection updatedycon = DBUtils.ExecuteDynamicObject(this.Context, updateBuilder.ToString());

                
                logger1.WriteLog("返工单开始查询：" + updatedycon.Count);

                if (updatedycon.Count > 0)
                {
                    
                    foreach (DynamicObject itme in updatedycon)
                    {
                        int counts = 0;
                        flag = true;
                        string finstockbillno = itme["FINSTOCKBILLNO"].ToString();

                        SelectInStockDate(counts, finstockbillno, finstockbillno);
                    }
                }
                #endregion
            }
            else
            {
                //排除返工单,不上查
                midtable = " KY_GetReportDataTable ";
                //midtable = " KY_GetReportDataTable where FINSTOCKBILLNO not in (select FINSTOCKBILLNO  from  KY_GetReportDataTable x left join  T_PRD_INSTOCK a on a.fbillno=x.FINSTOCKBILLNO inner join T_PRD_INSTOCKENTRY b on a.fid=b.fid left join T_PRD_MO m on m.fbillno=b.FMOBILLNO   where FINSTOCKFORMID='PRD_INSTOCK' and m.FBILLTYPE='0e74146732c24bec90178b6fe16a2d1c')";
            }

            StringBuilder InsertBuilder1 = new StringBuilder();
            InsertBuilder1.AppendLine("/*dialect*/");
            InsertBuilder1.AppendLine($" update a set a.FAGEDAYS=DATEDIFF(DAY, CONVERT(DATE, 入库日期),  CONVERT(DATE,'{FQueryDate}'))+{CurDateIsOne},FINSTOCKDATE=入库日期,FBILLDATE=入库日期 from KY_GetReportDataTable a inner join  instock1 b on  FInstockBillNo=b.入库单号 where  FMATERIALNUMBER=编码  and FLOTNO=b.原批号 ");

            StringBuilder newcol = new StringBuilder();
            InsertBuilder1.AppendLine($"select a.FMATERIALID as FMATERIALID1,* from ( select FMATERIALID,FMATERIALNUMBER,FMATERIALGROUPNAME,FMaterialTypeName,FStockOrgId,FSTOCKID,FSTOCKSTATUSID,FStockLocId,FSTOCKLOC,FLOTNO,FOwnerTypeId,FOWNERID,FKEEPERTYPEID,FKEEPERID,FBASEUNITID,FUnitID,CONVERT(date, FINSTOCKDATE) Fbilldatemin,DATEDIFF(DAY, CONVERT(date, FINSTOCKDATE),'{FQueryDate}')+{CurDateIsOne} AS FDayDiff" +
                $",case when FOWNERTYPEID='BD_OwnerOrg' then '业务组织'    when FOWNERTYPEID='BD_Customer' then '客户'    when FOWNERTYPEID='BD_Supplier' then '供应商' end FOwnerType ,case when FOWNERTYPEID='BD_OwnerOrg' then (select top 1 FNAME from T_ORG_ORGANIZATIONS_L where FORGID=FOWNERID and FLOCALEID=2052)    when FOWNERTYPEID='BD_Customer' then (select top 1 FNAME from T_BD_CUSTOMER_L where FCUSTID=FOWNERID and FLOCALEID=2052)    when FOWNERTYPEID='BD_Supplier' then (select top 1 FNAME from T_BD_SUPPLIER_L where FSUPPLIERID=FOWNERID and FLOCALEID=2052) end FOwner" +
                $",case when FKEEPERTYPEID='BD_KeeperOrg' then '业务组织'    when FKEEPERTYPEID='BD_Customer' then '客户'    when FKEEPERTYPEID='BD_Supplier' then '供应商'end FKeeperType,case when FKEEPERTYPEID='BD_KeeperOrg' then (select top 1 FNAME from T_ORG_ORGANIZATIONS_L where FORGID=FKEEPERID and FLOCALEID=2052)    when FKEEPERTYPEID='BD_Customer' then (select top 1 FNAME from T_BD_CUSTOMER_L where FCUSTID=FKEEPERID and FLOCALEID=2052)    when FKEEPERTYPEID='BD_Supplier' then (select top 1 FNAME from T_BD_SUPPLIER_L where FSUPPLIERID=FKEEPERID and FLOCALEID=2052) end FKeeper" +
                $",sum( CONVERT(decimal(18,6), fbaseqty)) Fsumfbaseqty,sum( CONVERT(decimal(18,6), fqty)) Fsumfqty,sum( CONVERT(decimal(18,6), famount)) Fsumfamount from   {midtable}  group by FMATERIALID,FMATERIALNUMBER,FMATERIALGROUPNAME,FMaterialTypeName,FStockOrgId,FSTOCKID,FSTOCKSTATUSID,FStockLocId,FSTOCKLOC,FLOTNO,FOwnerTypeId,FOWNERID,FKEEPERTYPEID,FKEEPERID,FBASEUNITID,FUnitID,FINSTOCKDATE ) a ");

            //动态列处理
            DynamicObject dyobject = filter.FilterParameter.CustomFilter;
            if (Convert.ToString(dyobject["FRadioGroup"]).Equals("A"))
            {
                StringBuilder InsertBuilder2 = new StringBuilder();
                InsertBuilder2.AppendLine(" select * ");


                DynamicObjectCollection InvAgeEntity = dyobject["InvAgeEntity"] as DynamicObjectCollection;
                int row = 1;
                foreach (DynamicObject dynamic in InvAgeEntity)
                {
                    string IntervalTxt = "IntervalTxt" + row++;


                    int DownDay = Convert.ToInt32(dynamic["DownDay"].ToString());
                    int UpperDay = Convert.ToInt32(dynamic["UpperDay"].ToString());
                    newcol.Append($",sum( CONVERT(decimal(18,6), {IntervalTxt}B)) as {IntervalTxt}B");
                    newcol.Append($",sum( CONVERT(decimal(18,6), {IntervalTxt}S)) as {IntervalTxt}S");
                    if (Convert.ToInt32(dynamic["IntervalDay"]) > 0)
                    {
                        InsertBuilder2.AppendLine($" ,case when  CONVERT(int, fagedays)>={DownDay} AND  CONVERT(int, fagedays)<={UpperDay} then CONVERT(decimal(18,6), fbaseqty) else 0.0 end {IntervalTxt}B");
                        InsertBuilder2.AppendLine($" ,case when  CONVERT(int, fagedays)>={DownDay} AND  CONVERT(int, fagedays)<={UpperDay} then CONVERT(decimal(18,6), fqty) else 0.0 end {IntervalTxt}S");
                    }
                    else
                    {
                        InsertBuilder2.AppendLine($" ,case when  CONVERT(int, fagedays)>={DownDay} then CONVERT(decimal(18,6), fbaseqty) else 0.0 end {IntervalTxt}B");
                        InsertBuilder2.AppendLine($" ,case when  CONVERT(int, fagedays)>={DownDay} then CONVERT(decimal(18,6), fqty) else 0.0 end {IntervalTxt}S");
                    }
                }
                InsertBuilder2.AppendLine("  from " + midtable);

                StringBuilder InsertBuilder3 = new StringBuilder();
                InsertBuilder3.AppendLine($" select  FMATERIALID,FMATERIALNUMBER,FMATERIALGROUPNAME,FMaterialTypeName,FStockOrgId,FSTOCKID,FSTOCKSTATUSID,FStockLocId,FSTOCKLOC,FLOTNO,FOwnerTypeId,FOWNERID,FKEEPERTYPEID,FKEEPERID,FBASEUNITID,FUnitID{newcol}" +
                    $",case when FOWNERTYPEID='BD_OwnerOrg' then '业务组织'    when FOWNERTYPEID='BD_Customer' then '客户'    when FOWNERTYPEID='BD_Supplier' then '供应商' end FOwnerType ,case when FOWNERTYPEID='BD_OwnerOrg' then (select top 1 FNAME from T_ORG_ORGANIZATIONS_L where FORGID=FOWNERID and FLOCALEID=2052)    when FOWNERTYPEID='BD_Customer' then (select top 1 FNAME from T_BD_CUSTOMER_L where FCUSTID=FOWNERID and FLOCALEID=2052)    when FOWNERTYPEID='BD_Supplier' then (select top 1 FNAME from T_BD_SUPPLIER_L where FSUPPLIERID=FOWNERID and FLOCALEID=2052) end FOwner" +
                    $",case when FKEEPERTYPEID='BD_KeeperOrg' then '业务组织'    when FKEEPERTYPEID='BD_Customer' then '客户'    when FKEEPERTYPEID='BD_Supplier' then '供应商' end FKeeperType,case when FKEEPERTYPEID='BD_KeeperOrg' then (select top 1 FNAME from T_ORG_ORGANIZATIONS_L where FORGID=FKEEPERID and FLOCALEID=2052)    when FKEEPERTYPEID='BD_Customer' then (select top 1 FNAME from T_BD_CUSTOMER_L where FCUSTID=FKEEPERID and FLOCALEID=2052)    when FKEEPERTYPEID='BD_Supplier' then (select top 1 FNAME from T_BD_SUPPLIER_L where FSUPPLIERID=FKEEPERID and FLOCALEID=2052) end FKeeper" +
                    $" from ({InsertBuilder2}  )   b1  group by FMATERIALID,FMATERIALNUMBER,FMATERIALGROUPNAME,FMaterialTypeName,FStockOrgId,FSTOCKID,FSTOCKSTATUSID,FStockLocId,FSTOCKLOC,FLOTNO,FOwnerTypeId,FOWNERID,FKEEPERTYPEID,FKEEPERID,FBASEUNITID,FUnitID ");


                InsertBuilder1.AppendLine($" left join ({InsertBuilder3}) b ");
                InsertBuilder1.AppendLine(" on a.FMATERIALID=b.FMATERIALID and a.FMATERIALNUMBER=b.FMATERIALNUMBER and a.FMATERIALGROUPNAME=b.FMATERIALGROUPNAME and a.FMaterialTypeName=b.FMaterialTypeName and a.FStockOrgId=b.FStockOrgId and a.FSTOCKID=b.FSTOCKID and a.FSTOCKSTATUSID=b.FSTOCKSTATUSID and a.FStockLocId=b.FStockLocId  and a.FSTOCKLOC=b.FSTOCKLOC  and a.FLOTNO=b.FLOTNO  and a.FOwnerTypeId=b.FOwnerTypeId and a.FOWNERID=b.FOWNERID and a.FKEEPERTYPEID=b.FKEEPERTYPEID and a.FKEEPERID=b.FKEEPERID and a.FBASEUNITID=b.FBASEUNITID and a.FUnitID=b.FUnitID ");

            }

            Logger logger = new Logger(Logpath, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
            logger.WriteLog("执行sql：" + InsertBuilder1.ToString());

            StringBuilder InsertBuilder = new StringBuilder();
            InsertBuilder.AppendLine("/*dialect*/");

            //真正查询数据的sql
            DynamicObjectCollection dycon = DBUtils.ExecuteDynamicObject(
                this.Context,
                InsertBuilder1.ToString()
                );
            int count = 1;
            foreach (dynamic dyn in dycon)
            {
                InsertBuilder.AppendFormat("Insert into {0} ", temp);
                InsertBuilder.Append(" ( ");
                for (int i = 0; i < Field.Length; i++)
                {
                    if (i == Field.Length - 1)
                    {
                        InsertBuilder.Append(Field[i] + ")");
                    }
                    else
                    {
                        InsertBuilder.Append(Field[i] + ",");
                    }
                }//对应列名称
                InsertBuilder.Append(" values ");
                InsertBuilder.Append("(");
                for (int i = 0; i < Field.Length; i++)
                {
                    var value = dyn[Field[i]];
                    if (i == 0)
                    {
                        InsertBuilder.Append("" + count++ + ",");
                    }
                    else if (i == Field.Length - 1)
                    {
                        InsertBuilder.Append("'" + value + "'");
                    }
                    else
                    {
                        InsertBuilder.Append("'" + value + "',");
                    }
                }
                InsertBuilder.AppendLine(")");
            }
            DBUtils.Execute(this.Context, InsertBuilder.ToString());
        }
        #endregion

        #region 返工入库逻辑
        /// <summary>
        /// 返工订单修改入库时间。反弓如库上查订单，订单下查领料，根据领料物料的维度匹配生产入库单。判断入库对应的订单类型是否返工，不是则返回此入库单日期，是则继续上查。
        /// 
        /// </summary>
        /// <param name="counts"></param>
        /// <param name="finstockbillno"></param>
        private void SelectInStockDate(int counts,string finstockbillno1, string finstockbillno)
        {
            counts++;
            string selectMotype = $"select top 1 FMOBILLNO,m.FBILLTYPE,a.FDATE from T_PRD_INSTOCK a inner join T_PRD_INSTOCKENTRY b on a.fid=b.fid left join T_PRD_MO m on m.fbillno=b.FMOBILLNO where a.FBILLNO='{finstockbillno1}'";
            DynamicObjectCollection motype = DBUtils.ExecuteDynamicObject(this.Context, selectMotype);
            if (motype.Count > 0)
            {
                //返工生产订单
                if (motype[0]["FBILLTYPE"].ToString().EqualsIgnoreCase("0e74146732c24bec90178b6fe16a2d1c"))
                {
                    string FMOBILLNO = motype[0]["FMOBILLNO"].ToString();
                    if (!FMoBillno.Contains(FMOBILLNO))
                    {
                        FMoBillno.Add(FMOBILLNO);
                        string selectdata = $"select * from (\r\n select b.FMOBILLNO,d.FBILLNO,c1.fdate,a.FBILLNO aFBILLNO from T_PRD_PICKMTRL a inner join T_PRD_PICKMTRLDATA b on a.fid=b.fid  inner join T_PRD_INSTOCKENTRY c on c.FMATERIALID=b.FMATERIALID and c.FSTOCKID=b.FSTOCKID and c.FSTOCKLOCID=b.FSTOCKLOCID and c.FLOT=b.FLOT inner join T_PRD_INSTOCK c1 on c1.fid=c.FID inner join T_PRD_INSTOCK d on c.fid=d.FID  where b.FMOBILLNO='{FMOBILLNO}'\r\n union all  select b.FMOBILLNO,d.FBILLNO,c1.fdate,a.FBILLNO aFBILLNO from T_PRD_FEEDMTRL a  inner join T_PRD_FEEDMTRLDATA b on a.fid=b.fid  inner join T_PRD_INSTOCKENTRY c on c.FMATERIALID=b.FMATERIALID and c.FSTOCKID=b.FSTOCKID and c.FSTOCKLOCID=b.FSTOCKLOCID and c.FLOT=b.FLOT inner join T_PRD_INSTOCK c1 on c1.fid=c.FID  inner join T_PRD_INSTOCK d on c.fid=d.FID  where b.FMOBILLNO='{FMOBILLNO}' ) z order by fdate desc";
                        DynamicObjectCollection selectdatas = DBUtils.ExecuteDynamicObject(this.Context, selectdata);
                        if (selectdatas.Count > 0)
                        {
                            SelectInStockDate(counts, selectdatas[0]["FBILLNO"].ToString(), finstockbillno);
                        }
                    }   
                }
                else if (counts > 1 && flag)
                {
                    DBUtils.Execute(this.Context, $" update KY_GetReportDataTable set FINSTOCKDATE='{motype[0]["FDATE"].ToString()}' where FINSTOCKBILLNO='{finstockbillno}'");
                    Logger logger = new Logger(Logpath, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
                    logger.WriteLog("更新sql：" + $" update KY_GetReportDataTable set FINSTOCKDATE='{motype[0]["FDATE"].ToString()}' where FINSTOCKBILLNO='{finstockbillno}'");
                    flag = false;
                }
            }
        }
        #endregion 返工入库逻辑

        #region  构建报表核心
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);
            this.FilterParameter(filter); //给过滤条件赋值
            var orderBy = filter.FilterParameter.SortString;//排序字段
            if (orderBy.Length > 0)
            {
                base.KSQL_SEQ = string.Format(this.KSQL_SEQ, orderBy);

            }
            else
            {
                base.KSQL_SEQ = string.Format(this.KSQL_SEQ, "FMATERIALNUMBER desc");
            }
            using (new SessionScope())
            {
                //创建临时表-
                IDBService dbService =
                    Kingdee.BOS.App.ServiceHelper.GetService<Kingdee.BOS.Contracts.IDBService>();
                string[] arrTableName = null;
                string tmpTableName = String.Empty;
                arrTableName = dbService.CreateTemporaryTableName(this.Context, 1);
                tmpTableName = arrTableName[0];
                DBUtils.ExecuteDynamicObject(this.Context, this.CreateMainTempSql(tmpTableName));
                this.InventoryDetailSummarySql(tmpTableName, filter);
                StringBuilder sql = new StringBuilder();
                sql.AppendLine(" SELECT  ");
                for (int i = 0; i < Field.Length; i++)
                {
                    if (i == Field.Length - 1)
                    {
                        sql.AppendLine(Field[i]);
                    }
                    else
                    {
                        sql.AppendLine(Field[i] + ",");
                       
                    }
                }
                sql.AppendFormat(" ,{0} ", base.KSQL_SEQ);
                sql.AppendFormat(" INTO {0}  ", tableName);
                sql.AppendFormat(" FROM {0}  ", tmpTableName);
                //sql.AppendFormat(" order by {0}  ", filter.FilterParameter.SortString);



                DBUtils.Execute(this.Context, sql.ToString());
                //删除表
                ITemporaryTableService service2 =
                    Kingdee.BOS.Contracts.ServiceFactory.GetService<Kingdee.BOS.Contracts.ITemporaryTableService>(
                        this.Context
                    );
                service2.DropTable(base.Context, new HashSet<string>(arrTableName));
            }
        }

        #endregion

        #region 设置列名
        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            ReportHeader header = new ReportHeader();
            //header.AddChild("FIDENTITYID", new LocaleValue("序号")).ColIndex = 0;
            for (int i = 1; i < Field.Length; i++)
            {
                string value = Field[i];
                switch (Field[i])
                {
                    case "FMATERIALNUMBER":
                        value = "物料编码";
                        break;
                    case "FMaterialTypeName":
                        value = "存货类型";
                        break;
                    case "Fbilldatemin":
                        value = "最早入库日期";
                        break;
                    case "FDayDiff":
                        value = "库龄天数";
                        break;
                    case "Fsumfbaseqty":
                        value = "数量(基本)";
                        break;
                    case "Fsumfqty":
                        value = "数量(库存)";
                        break;
                    case "Fsumfamount":
                        value = "总金额";
                        break;
                    case "FOwnerType":
                        value = "货主类型";
                        break;
                    case "FOwner":
                        value = "货主";
                        break;
                    case "FKeeperType":
                        value = "保管者类型";
                        break;
                    case "FKeeper":
                        value = "保管者";
                        break;
                    case "FSTOCKLOC":
                        value = "仓位值集";
                        break;
                    case "FLOTNO":
                        value = "批号";
                        break;



                    case "FMATERIALID":
                        value = "物料内码";
                        break;
                    case "FStockOrgId":
                        value = "库存组织内码";
                        break;
                    case "FSTOCKID":
                        value = "仓库内码";
                        break;
                    case "FSTOCKSTATUSID":
                        value = "库存状态内码";
                        break;
                    case "FStockLocId":
                        value = "仓位内码";
                        continue;
                    case "FBASEUNITID":
                        value = "基本单位内码";
                        break;
                    case "FUNITID":
                        value = "库存单位内码";
                        break;

    
                }
                if (keyValues.TryGetValue(Field[i], out string value1))
                {
                    if (Field[i].Last().Equals('B'))
                    {
                        value = value1 + "&数量(基本)";
                    }
                    else if (Field[i].Last().Equals('S'))
                    {
                        value = value1 + "&数量(库存)";
                    }
                    header.AddChild(Field[i], new LocaleValue(value)).ColIndex = i + 20;
                }

                //设置列与类型，日期bos处理
                if (value.Contains("数量"))
                {
                    Kingdee.BOS.Core.List.ListHeader listHeader = header.AddChild(Field[i], new LocaleValue(value));
                    listHeader.ColIndex = i + 20;
                    listHeader.ColType = SqlStorageType.SqlNumeric;

                }
                else if (value.Contains("最早入库日期"))
                {
                    Kingdee.BOS.Core.List.ListHeader listHeader = header.AddChild(Field[i], new LocaleValue(value));
                    listHeader.ColIndex = i + 20;
                    listHeader.ColType = SqlStorageType.SqlSmalldatetime;
                }
                else
                {
                    header.AddChild(Field[i], new LocaleValue(value)).ColIndex = i + 20;
                }



            }
            return header;
        }
        #endregion 设置列名

        #region 过滤条件
        private void FilterParameter(IRptParams filter)
        {
            DynamicObject dyobject = filter.FilterParameter.CustomFilter;
            if (dyobject != null)
            {
                /*if (dyobject["StockOrgId"] != null)
                {
                    F_UJED_StockOrg = Convert.ToString(dyobject["StockOrgId"]);

                    string[] strings = F_UJED_StockOrg.Split(',');
                    StringBuilder sb = new StringBuilder();
                    sb.Append("(");
                    for (int i = 0; i < strings.Length; i++)
                    {
                        if (i == strings.Length - 1)
                        {
                            sb.Append("'" + strings[i] + "'");
                        }
                        else
                        {
                            sb.Append("'" + strings[i] + "',");
                        }

                    }
                    sb.Append(")");
                    F_UJED_StockOrgWhere = sb.ToString();




                }*/
                if (Convert.ToInt32(dyobject["MaterialFrom_Id"]) != 0)
                {
                    DynamicObject MaterialFrom = (DynamicObject)dyobject["MaterialFrom"];
                    DynamicObject MaterialTo = (DynamicObject)dyobject["MaterialTo"];
                    F_UJED_MaterialRange = MaterialFrom["Number"] + "(" + MaterialFrom["Name"] + ")--" + MaterialTo["Number"] + "(" + MaterialTo["Name"] + ")";

                    if (MaterialFrom["Number"].ToString().Equals(MaterialTo["Number"].ToString()))
                    {
                        F_UJED_MaterialWhere = "('" + MaterialFrom["Number"].ToString() + "')";
                    }
                    else
                    {
                        DynamicObjectCollection Materialdys = DBUtils.ExecuteDynamicObject(this.Context, $"SELECT Fnumber FROM  T_BD_MATERIAL where FNUMBER BETWEEN '{MaterialFrom["Number"].ToString()}' and  '{MaterialTo["Number"].ToString()}' ORDER  by FNUMBER");
                        StringBuilder sb = new StringBuilder();
                        sb.Append("(");
                        for (int i = 0; i < Materialdys.Count; i++)
                        {
                            if (i == Materialdys.Count - 1)
                            {
                                sb.Append("'" + Materialdys[i]["Fnumber"] + "'");
                            }
                            else
                            {
                                sb.Append("'" + Materialdys[i]["Fnumber"] + "',");
                            }
                        }
                        sb.Append(")");
                        F_UJED_MaterialWhere = sb.ToString();
                    }

                }
                if (Convert.ToInt32(dyobject["StockFrom_Id"]) != 0)
                {
                    DynamicObject StockFrom = (DynamicObject)dyobject["StockFrom"];
                    DynamicObject StockTo = (DynamicObject)dyobject["StockTo"];
                    F_UJED_StockRange = StockFrom["Number"] + "(" + StockFrom["Name"] + ")--" + StockTo["Number"] + "(" + StockTo["Name"] + ")";
                    if (StockFrom["Number"].ToString().Equals(StockTo["Number"].ToString()))
                    {
                        F_UJED_StockWhere = "('" + StockFrom["Number"].ToString() + "')";
                    }
                    else
                    {
                        DynamicObjectCollection Stockdys = DBUtils.ExecuteDynamicObject(this.Context, $"SELECT Fnumber FROM  T_BD_STOCK where FNUMBER BETWEEN '{StockFrom["Number"].ToString()}' and  '{StockTo["Number"].ToString()}' ORDER  by FNUMBER");
                        StringBuilder sb = new StringBuilder();
                        sb.Append("(");
                        for (int i = 0; i < Stockdys.Count; i++)
                        {
                            if (i == Stockdys.Count - 1)
                            {
                                sb.Append("'" + Stockdys[i]["Fnumber"] + "'");
                            }
                            else
                            {
                                sb.Append("'" + Stockdys[i]["Fnumber"] + "',");
                            }
                        }
                        sb.Append(")");
                        F_UJED_StockWhere = sb.ToString();
                    }
                }
                if (Convert.ToInt32(dyobject["LotFrom_Id"]) != 0)
                {
                    DynamicObject LotFrom = (DynamicObject)dyobject["LotFrom"];
                    DynamicObject LotTo = (DynamicObject)dyobject["LotTo"];
                    F_UJED_LotRange = LotFrom["Number"] + "(" + LotFrom["Name"] + ")--" + LotTo["Number"] + "(" + LotTo["Name"] + ")";

                    if (LotFrom["Number"].ToString().Equals(LotTo["Number"].ToString()))
                    {
                        F_UJED_LotWhere = "('" + LotFrom["Number"].ToString() + "')";
                    }
                    else
                    {
                        DynamicObjectCollection Lotdys = DBUtils.ExecuteDynamicObject(this.Context, $"SELECT Fnumber FROM  T_BD_LOTMASTER where FNUMBER BETWEEN '{LotFrom["Number"].ToString()}' and  '{LotTo["Number"].ToString()}' ORDER  by FNUMBER");
                        StringBuilder sb = new StringBuilder();
                        sb.Append("(");
                        for (int i = 0; i < Lotdys.Count; i++)
                        {
                            if (i == Lotdys.Count - 1)
                            {
                                sb.Append("'" + Lotdys[i]["Fnumber"] + "'");
                            }
                            else
                            {
                                sb.Append("'" + Lotdys[i]["Fnumber"] + "',");
                            }
                        }
                        sb.Append(")");
                        F_UJED_LotWhere = sb.ToString();
                    }


                }

                CurDateIsOne = Convert.ToBoolean(dyobject["CurDateIsOne"]) ? 1 : 0;
                Field = Field.Where(f => !f.Contains("IntervalTxt")).ToArray();
                if (Convert.ToString(dyobject["FRadioGroup"]).Equals("A"))
                {
                    DynamicObjectCollection InvAgeEntity = dyobject["InvAgeEntity"] as DynamicObjectCollection;
                    keyValues = new Dictionary<string, string>();
                    int row = 1;
                    foreach (DynamicObject dynamic in InvAgeEntity)
                    {
                        string IntervalTxt = "IntervalTxt" + row++;
                        string Interval = dynamic["IntervalTxt"].ToString();
                        keyValues.Add(IntervalTxt + "B", Interval);
                        keyValues.Add(IntervalTxt + "S", Interval);

                        //设置动态列数组
                        Array.Resize(ref Field, Field.Length + 2);
                        Array.Resize(ref FieldType, FieldType.Length + 2);
                        Field[Field.Length - 2] = IntervalTxt + "B";
                        Field[Field.Length - 1] = IntervalTxt + "S";
                        FieldType[FieldType.Length - 2] = "DECIMAL(23,10)";
                        FieldType[FieldType.Length - 1] = "DECIMAL(23,10)";


                    }
                }

                Fisrework=Convert.ToBoolean(dyobject["Fisrework"]);

                FQueryDate = dyobject["FQueryDate"].ToString();

                
            }
        }
        #endregion

        #region 过滤条件返回
        public override ReportTitles GetReportTitles(IRptParams filter)
        {
            ReportTitles titles = new ReportTitles();
            //titles.AddTitle("F_UJED_StockOrg", F_UJED_StockOrg);
            titles.AddTitle("F_UJED_MaterialRange", F_UJED_MaterialRange);
            titles.AddTitle("F_UJED_StockRange", F_UJED_StockRange);
            titles.AddTitle("F_UJED_LotRange", F_UJED_LotRange);

            return titles;
        }
        #endregion
    }
}
