using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;    
using System.Text;

namespace BD.Standard.KY.ReportServicePlugIn
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("销售订单基准价报表服务插件")]
    public class SaleOrderReportPlugIn : SysReportBaseService
    {
        //合同号
        private string F_UJED_BeginDatetime = "";
        private string F_UJED_EndDatetime = "";
        private string F_UJED_PriceDatetime = "";
        private string F_UJED_Supplier = "";
        private string F_UJED_Material = "";
        private string F_UJED_OrgId = "";
        private string F_UJED_OrgName = "";
        private Boolean Fissupplier = true;

        private static readonly string Logpath = @"D:\KYLog\reportLog\" + DateTime.Now.ToString("yyyyMM");

        #region 设置报表
        public override void Initialize()
        {
            base.Initialize();
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            this.ReportProperty.ReportName = new LocaleValue(
                "销售订单基准价报表",
                base.Context.UserLocale.LCID
            );
            this.IsCreateTempTableByPlugin = true;
            this.ReportProperty.IsGroupSummary = true;
            this.ReportProperty.IsUIDesignerColumns = false;
            this.ReportProperty.SimpleAllCols = false;
            this.ReportProperty.IdentityFieldName = "FIDENTITYID";
            this.SetDecimalControl();
        }
        #endregion 设置报表

        #region 汇总
        public override List<SummaryField> GetSummaryColumnInfo(IRptParams filter)
        {
            var result = base.GetSummaryColumnInfo(filter);
            result.Add(new SummaryField("fqty", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FPRICE", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FTAXPRICE", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FTAXPRICE2", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("Fpricediff", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FALLAMOUNT", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FALLAMOUNT_LC", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FAMOUNT", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("FAMOUNT_LC", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));

            return result;
        }
        #endregion

        private void SetDecimalControl()
        {
            List<DecimalControlField> list = new List<DecimalControlField>();

            this.ReportProperty.DecimalControlFieldList = list;
        }

        

        private string[] Field = { "orderfid", "Fbilltype", "fbillno", "fdate", "FSEQ", "Fnote", "FCustId", "FCustIdNumber", "FCustIdName", "FMATERIALID", "FMATERIALNumber", "FMATERIALName", "fqty", "FTaxRate", "FPRICE", "FTAXPRICE", "FTAXPRICE2", "Fpricediff", "FbaseBillno", "FbaseSeq", "FbaseNote", "F_QCPR_PROJECT","FuserName", "fdeptname",  "FUNITID", "FSETTLECURRID", "FLOCALCURRID", "FALLAMOUNT", "FALLAMOUNT_LC", "FAMOUNT", "FAMOUNT_LC", "Fmg1lFNAME",  "Fmg2lFNAME","Fmg3lFNAME", "FGIVEAWAY", "FPURCHASEORGName", "Fpayname", "FDOCUMENTSTATUS", "FCLOSESTATUS", "FMRPCLOSESTATUS", "Fproductline", "Fproductline1" };

        

        private string[] FieldType = { "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(max)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "decimal(18,8)", "decimal(18,8)", "decimal(18,8)", "decimal(18,8)", "decimal(18,8)", "decimal(18,8)", "varchar(100)", "varchar(100)", "varchar(max)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)",  "varchar(100)", "varchar(100)", "decimal(18,8)", "decimal(18,8)", "decimal(18,8)", "decimal(18,8)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)","varchar(100)", "varchar(100)" };

        private string[] Field1 = { "订单内码","采购类型", "单据编号", "订单日期","订单行号","明细备注","客户内码", "客户编码", "客户名称", "物料内码", "物料编码", "物料名称", "采购数量", "税率%", "单价", "含税单价", "基准价(含税)", "含税单价差", "基准价单据编号", "基准价明细行号", "基准价明细备注","项目号","销售创建人","销售部门","销售单位", "结算币别内码", "本位币内码", "价税合计", "价税合计(本位币)", "金额", "金额(本位币)", "物料分组&小类", "物料分组&中类", "物料分组&大类", "是否赠品","销售组织","付款条件","单据状态","关闭状态","业务状态", "产品线内码", "基准价产品线内码" };


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
        public void InventoryDetailSummarySql(string temp)
        {
            StringBuilder t1where = new StringBuilder();
            StringBuilder t2where = new StringBuilder();
            t1where.AppendLine(" where 1=1 ");
            if (!string.IsNullOrWhiteSpace(F_UJED_BeginDatetime))
            {
                t1where.AppendLine($" and a.fdate>='{F_UJED_BeginDatetime}' ");
            }
            if (!string.IsNullOrWhiteSpace(F_UJED_EndDatetime))
            {
                t1where.AppendLine($" and a.fdate<='{F_UJED_EndDatetime}' ");
            }
            if (!string.IsNullOrWhiteSpace(F_UJED_Supplier))
            {
                t1where.AppendLine($" and s.FNUMBER='{F_UJED_Supplier}' ");
                t2where.AppendLine($" and s.FNUMBER='{F_UJED_Supplier}' ");
            }
            if (!string.IsNullOrWhiteSpace(F_UJED_Material))
            {
                t1where.AppendLine($" and m.fnumber='{F_UJED_Material}' ");
                t2where.AppendLine($" and m.fnumber='{F_UJED_Material}' ");
            }
            if (!string.IsNullOrWhiteSpace(F_UJED_OrgId))
            {
                t1where.AppendLine($" and FSaleOrgId='{F_UJED_OrgId}' ");
                t2where.AppendLine($" and FSaleOrgId='{F_UJED_OrgId}' ");
            }
            //基准价匹配是否包含供应商
            string isSupplierBB1Where = Fissupplier ? "and a.FCustId=BB1.FCustId" : "";
            string isSupplierCC1Where = Fissupplier ? "and a.FCustId=CC1.FCustId" : "";


            StringBuilder sql1 = new StringBuilder();
            #region BB1 小于基准时间表
            sql1.AppendLine($"/*dialect*/WITH  BB1 AS ( ");
            sql1.AppendLine(" SELECT b.FMATERIALID, a.FCustId, a.FDATE, b.fentryid, a.FBILLNO, b.FSEQ, b.FNOTE, ");
            sql1.AppendLine(" a.F_QCPR_Base Fproductline1,c.FTAXPRICE,  ");
            sql1.AppendLine(" ROW_NUMBER() OVER(PARTITION BY b.FMATERIALID, a.FCustId ORDER BY a.FDATE DESC, b.fentryid ASC ) AS RowNum ");
            sql1.AppendLine(" FROM T_SAL_ORDER a ");
            sql1.AppendLine(" INNER JOIN T_SAL_ORDERENTRY b ON a.fid = b.FID ");
            sql1.AppendLine(" INNER JOIN T_SAL_ORDERENTRY_F c ON b.FENTRYID = c.FENTRYID ");
            sql1.AppendLine(" left join T_BD_MATERIAL m on m.FMATERIALID=b.FMATERIALID ");
            sql1.AppendLine(" left join T_BD_CUSTOMER s on s.FCustId=a.FCustId ");
            sql1.AppendLine($" WHERE a.FDATE < '{F_UJED_PriceDatetime}' {t2where} AND c.FTAXPRICE > 0 )");

            #endregion BB1 小于基准时间表

            #region CC1 大于基准时间表
            sql1.AppendLine($",CC1 AS ( ");
            sql1.AppendLine(" SELECT b.FMATERIALID, a.FCustId, a.FDATE, b.fentryid, a.FBILLNO, b.FSEQ, b.FNOTE, ");
            sql1.AppendLine(" a.F_QCPR_Base Fproductline1,c.FTAXPRICE,  ");
            sql1.AppendLine(" ROW_NUMBER() OVER(PARTITION BY b.FMATERIALID, a.FCustId ORDER BY a.FDATE, b.fentryid DESC ) AS RowNum ");
            sql1.AppendLine(" FROM T_SAL_ORDER a ");
            sql1.AppendLine(" INNER JOIN T_SAL_ORDERENTRY b ON a.fid = b.FID ");
            sql1.AppendLine(" INNER JOIN T_SAL_ORDERENTRY_F c ON b.FENTRYID = c.FENTRYID ");
            sql1.AppendLine(" left join T_BD_MATERIAL m on m.FMATERIALID=b.FMATERIALID ");
            sql1.AppendLine(" left join T_BD_CUSTOMER s on s.FCustId=a.FCustId ");
            sql1.AppendLine($" WHERE a.FDATE > '{F_UJED_PriceDatetime}' {t2where} AND c.FTAXPRICE > 0 )");
            #endregion CC1 大于基准时间表

            #region 主表
            sql1.AppendLine(" SELECT  a.fid as orderfid,x.FNAME Fbilltype,a.fbillno,a.fdate,b.FSEQ,b.FNOTE,s.FCustId, ");
            sql1.AppendLine("  s.FNUMBER as FCustIdNumber,sl.fname as FCustIdName,m.FMATERIALID,m.fnumber as FMATERIALNumber,ml.FNAME as FMATERIALName, ");
            sql1.AppendLine(" fqty,c.FTaxRate,FPRICE,c.FTAXPRICE, ");
            sql1.AppendLine(" COALESCE(BB1.FTAXPRICE, CC1.FTAXPRICE, c.FTAXPRICE) AS FTAXPRICE2, ");
            sql1.AppendLine(" c.FTAXPRICE-COALESCE(BB1.FTAXPRICE, CC1.FTAXPRICE, c.FTAXPRICE) Fpricediff, ");
            sql1.AppendLine(" COALESCE(BB1.FBILLNO, CC1.FBILLNO, '') FbaseBillno,COALESCE(CAST(BB1.FSEQ AS VARCHAR(10)), ");
            sql1.AppendLine(" CAST(CC1.FSEQ AS VARCHAR(10)), '') FbaseSeq,COALESCE(BB1.FNOTE, CC1.FNOTE, '') FbaseNote, ");
            sql1.AppendLine(" COALESCE(BB1.Fproductline1, CC1.Fproductline1, '') Fproductline1, ");
            sql1.AppendLine(" a.F_QCPR_Base Fproductline, ");
            sql1.AppendLine(" a.F_QCPR_PROJECT, ");
            sql1.AppendLine(" u.Fname FuserName,isnull(d.FNAME,'') fdeptname,FUNITID,FSETTLECURRID,FLOCALCURRID,FALLAMOUNT,FALLAMOUNT_LC,FAMOUNT,FAMOUNT_LC, ");
            sql1.AppendLine(" isnull(mg1l.FNAME,'') Fmg1lFNAME,isnull(mg2l.FNAME,'') Fmg2lFNAME,isnull(mg3l.FNAME,'') Fmg3lFNAME, ");
            sql1.AppendLine(" case when FIsFree=1 then '是' else '否' end  FGIVEAWAY, ");
            sql1.AppendLine(" ol.FNAME FPURCHASEORGName, ");
            sql1.AppendLine(" isnull(pay.FNAME,'') Fpayname, ");
            sql1.AppendLine(" fol.FCAPTION FDOCUMENTSTATUS, ");
            sql1.AppendLine(" case when FCLOSESTATUS='A' THEN '未关闭' else'已关闭' end FCLOSESTATUS, ");
            sql1.AppendLine(" case when FMRPCLOSESTATUS='A' THEN '正常' else'业务关闭' end FMRPCLOSESTATUS ");
            //sql1.AppendLine("  ");
            sql1.AppendLine(" from T_SAL_ORDER a  inner join T_SAL_ORDERFIN F on a.fid=F.FID ");
            sql1.AppendLine(" inner join T_SAL_ORDERENTRY b on a.fid=b.FID ");
            sql1.AppendLine(" inner join T_SAL_ORDERENTRY_F c on b.FENTRYID=c.FENTRYID ");
            sql1.AppendLine(" inner join T_SAL_ORDERENTRY_D c1 on b.FENTRYID=c1.FENTRYID ");
            sql1.AppendLine(" left join T_BD_CUSTOMER s on s.FCustId=a.FCustId ");
            sql1.AppendLine(" left join T_BD_CUSTOMER_L sl on s.FCustId=sl.FCustId   and sl.FLOCALEID=2052 ");
            sql1.AppendLine(" left join T_BD_MATERIAL m on m.FMATERIALID=b.FMATERIALID ");
            sql1.AppendLine(" left join T_BD_MATERIAL_L ml on m.FMATERIALID=ml.FMATERIALID   and ml.FLOCALEID=2052 ");
            sql1.AppendLine(" left join T_ORG_ORGANIZATIONS o on o.FORGID= a.FSaleOrgId ");
            sql1.AppendLine(" left join T_ORG_ORGANIZATIONS_L ol on o.FORGID= ol.FORGID    and ol.FLOCALEID=2052   ");
            sql1.AppendLine(" left join T_BAS_BILLTYPE_L x on x.FBILLTYPEID= a.FBILLTYPEID ");
            sql1.AppendLine(" left join T_SEC_USER u on u.FUSERID=a.FCREATORID ");
            sql1.AppendLine(" left join T_BD_DEPARTMENT_L d on d.FDEPTID=FSaleDeptId ");
            sql1.AppendLine(" left join T_BD_MATERIALGROUP mg1 on mg1.FID=m.FMATERIALGROUP left join T_BD_MATERIALGROUP_L mg1l on mg1l.FID=mg1.fid ");
            sql1.AppendLine(" left join T_BD_MATERIALGROUP mg2 on mg2.FID=mg1.FPARENTID left join T_BD_MATERIALGROUP_L mg2l on mg2l.FID=mg2.fid ");
            sql1.AppendLine(" left join T_BD_MATERIALGROUP mg3 on mg3.FID=mg2.FPARENTID left join T_BD_MATERIALGROUP_L mg3l on mg3l.FID=mg3.fid ");
            sql1.AppendLine($" LEFT JOIN (select* from  BB1 where RowNum= 1) BB1 ON b.FMATERIALID = BB1.FMATERIALID {isSupplierBB1Where} ");
            sql1.AppendLine($" LEFT JOIN (select* from  CC1 where RowNum= 1) CC1 ON b.FMATERIALID = CC1.FMATERIALID {isSupplierCC1Where} ");
            sql1.AppendLine(" left join T_BD_RecCondition_L pay on pay.FID=f.FRecConditionId ");
            sql1.AppendLine(" left join T_META_FORMENUMITEM fo on  fo.fid='14039efd-6350-4eab-b482-c1c6bcdf914b' and fo.FVALUE= a.FDOCUMENTSTATUS ");
            sql1.AppendLine(" left join T_META_FORMENUMITEM_L fol on fo.FENUMID=fol.FENUMID  ");
            //sql1.AppendLine("  ");
            sql1.AppendLine($"{t1where}");
            #endregion 主表

            Logger logger = new Logger(Logpath, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
            logger.WriteLog("执行sql：" + sql1.ToString());

            #region 插入临时表
            StringBuilder InsertBuilder = new StringBuilder();
            InsertBuilder.AppendLine("/*dialect*/");
            //真正查询数据的sql
            DynamicObjectCollection dycon = DBUtils.ExecuteDynamicObject(
                this.Context,
                string.Format(sql1.ToString())
                );
            int count = 1;
            foreach (dynamic dyn in dycon)
            {
                InsertBuilder.AppendFormat("Insert into {0}  ", temp);
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
                    if (i == 0)
                    {
                        InsertBuilder.Append("" + count++ + ",");
                    }
                    else if (i == Field.Length - 1)
                    {
                        InsertBuilder.Append("'" + dyn[Field[i]].ToString().Replace("'", "''") + "'");
                    }
                    else
                    {
                        InsertBuilder.Append("'" + dyn[Field[i]].ToString().Replace("'", "''") + "',");
                    }
                }
                InsertBuilder.AppendLine(")");
            }
            DBUtils.Execute(this.Context, InsertBuilder.ToString());
            #endregion 插入临时表

        }
        #endregion

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
                base.KSQL_SEQ = string.Format(base.KSQL_SEQ, "fbillno ASC");
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
                this.InventoryDetailSummarySql(tmpTableName);
                StringBuilder sql = new StringBuilder();
                sql.AppendFormat(" SELECT ");
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
                sql.AppendFormat(" ,{0}  ", base.KSQL_SEQ);
                sql.AppendFormat(" INTO {0}   ", tableName);
                sql.AppendFormat(" FROM {0}   ", tmpTableName);
                sql.AppendFormat(" WHERE 1=1   ");
                if (!string.IsNullOrWhiteSpace(filter.FilterParameter.FilterString))
                {
                    sql.AppendFormat("  and {0}", filter.FilterParameter.FilterString);

                }




                DBUtils.ExecuteDynamicObject(this.Context, sql.ToString());
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
            for (int i = 1; i < Field1.Length; i++)
            {
                header.AddChild(Field[i], new LocaleValue(Field1[i])).ColIndex = i;
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
                F_UJED_BeginDatetime = dyobject["F_UJED_BeginDatetime"] != null ? dyobject["F_UJED_BeginDatetime"].ToString() : "";
                F_UJED_EndDatetime = dyobject["F_UJED_EndDatetime"] != null ? dyobject["F_UJED_EndDatetime"].ToString() : "";
                F_UJED_PriceDatetime = dyobject["F_UJED_PriceDatetime"] != null ? dyobject["F_UJED_PriceDatetime"].ToString() : "";

                F_UJED_Supplier = dyobject["F_UJED_Supplier"] != null ? ((DynamicObject)dyobject["F_UJED_Supplier"])["Number"].ToString() : "";
                F_UJED_Material = dyobject["F_UJED_Material"] != null ? ((DynamicObject)dyobject["F_UJED_Material"])["Number"].ToString() : "";
                F_UJED_OrgId = dyobject["F_UJED_OrgId"] != null ? ((DynamicObject)dyobject["F_UJED_OrgId"])["ID"].ToString() : "";
                F_UJED_OrgName = dyobject["F_UJED_OrgId"] != null ? ((DynamicObject)dyobject["F_UJED_OrgId"])["Name"].ToString() : "";
                Fissupplier = Convert.ToBoolean(dyobject["Fissupplier"]);

            }
        }
        #endregion

        #region 过滤条件返回
        public override ReportTitles GetReportTitles(IRptParams filter)
        {
            ReportTitles titles = new ReportTitles();
            titles.AddTitle("F_UJED_BeginDatetime", F_UJED_BeginDatetime);
            titles.AddTitle("F_UJED_EndDatetime", F_UJED_EndDatetime);
            titles.AddTitle("F_UJED_PriceDatetime", F_UJED_PriceDatetime);
            titles.AddTitle("F_UJED_Supplier", F_UJED_Supplier);
            titles.AddTitle("F_UJED_Material", F_UJED_Material);
            titles.AddTitle("F_UJED_OrgId", F_UJED_OrgName);

            return titles;
        }
        #endregion
    }
}
