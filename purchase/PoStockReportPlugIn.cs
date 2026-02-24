

using BD.Standard.KY.ReportServicePlugIn;
using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Core.Report.PlugIn;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Text;

namespace BD.Standard.KY.ReportServicePlugIn
{
    [HotUpdate]
    [Description("采购入库基准价报表服务插件")]
    public class PoStockReportPlugIn : SysReportBaseService
    {
        private string F_UJED_BeginDatetime = "";

        private string F_UJED_EndDatetime = "";

        private string F_UJED_PriceDatetime = "";

        private string F_UJED_Supplier = "";

        private string F_UJED_Material = "";

        private string F_UJED_OrgId = "";

        private string F_UJED_OrgName = "";

        private Boolean Fissupplier = true;

        private static readonly string Logpath = "D:\\KYLog\\reportLog\\" + DateTime.Now.ToString("yyyyMM");

        private string[] Field =
        {
        "orderfid", "Fbilltype", "fbillno", "fdate", "FSEQ", "Fnote", "FSUPPLIERID", "FSUPPLIERNumber", "FSUPPLIERName", "FMATERIALID",
        "FMATERIALNumber", "FMATERIALName", "FREALQTY", "FTaxRate", "FPRICE", "FTAXPRICE", "FTAXPRICE2", "FTAXPRICE3", "FTAXPRICE4", "Fpricediff",
        "FbaseBillno", "FbaseSeq", "FbaseNote", "FuserName", "fdeptname", "FUNITID", "FSETTLECURRID", "FLOCALCURRID", "FALLAMOUNT", "FALLAMOUNT_LC",
        "FAMOUNT", "FAMOUNT_LC", "Fmg1lFNAME", "Fmg2lFNAME", "Fmg3lFNAME", "FGIVEAWAY", "FPURCHASEORGName", "Fpayname", "FDOCUMENTSTATUS", "fbillnoZG",
        "fseqZG", "FCommentZG", "fbillnoCW", "fseqCW", "FCommentCW", "FREQUIRESTAFFID", "F_QCPR_PROJECT", "Fproductline"
        };

        private string[] FieldType =
        {
        "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(max)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)",
        "varchar(100)", "varchar(100)", "decimal(18,8)", "decimal(18,8)", "decimal(18,8)", "decimal(18,8)", "decimal(18,8)", "decimal(18,8)", "decimal(18,8)", "decimal(18,8)",
        "varchar(100)", "varchar(100)", "varchar(max)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "decimal(18,8)", "decimal(18,8)",
        "decimal(18,8)", "decimal(18,8)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)", "varchar(100)",
        "varchar(100)", "varchar(max)", "varchar(100)", "varchar(100)", "varchar(max)", "varchar(100)", "varchar(100)", "varchar(100)"
        };

        private string[] Field1 =
        {
        "订单内码", "采购类型", "单据编号", "订单日期", "订单行号", "明细备注", "供应商内码", "供应商编码", "供应商名称", "物料内码",
        "物料编码", "物料名称", "采购数量", "税率%", "单价", "含税单价", "基准价(含税)", "暂估应付(含税)", "财务应付(含税)", "含税单价差",
        "基准价单据编号", "基准价明细行号", "基准价明细备注", "采购创建人", "采购部门", "采购单位", "结算币别内码", "本位币内码", "价税合计", "价税合计(本位币)",
        "金额", "金额(本位币)", "物料分组&小类", "物料分组&中类", "物料分组&大类", "是否赠品", "采购组织", "付款条件", "单据状态", "暂估单据编号",
        "暂估明细行号", "暂估明细备注", "财务单据编号", "财务明细行号", "财务明细备注", "需求人内码", "项目号内码","产品线内码"
        };



        public override void Initialize()
        {
            base.Initialize();
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            this.ReportProperty.ReportName = new LocaleValue(
                "采购入库基准价报表",
                base.Context.UserLocale.LCID
            );
            this.IsCreateTempTableByPlugin = true;
            this.ReportProperty.IsGroupSummary = true;
            this.ReportProperty.IsUIDesignerColumns = false;
            this.ReportProperty.SimpleAllCols = false;
            this.ReportProperty.IdentityFieldName = "FIDENTITYID";
            this.SetDecimalControl();
        }

        public override List<SummaryField> GetSummaryColumnInfo(IRptParams filter)
        {
            var result = base.GetSummaryColumnInfo(filter);
            result.Add(new SummaryField("FREALQTY", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
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

        private void SetDecimalControl()
        {
            List<DecimalControlField> decimalControlFieldList = new List<DecimalControlField>();
            ((AbstractSysReportServicePlugIn)this).ReportProperty.DecimalControlFieldList = decimalControlFieldList;
        }

        public string CreateMainTempSql(string tmpTableName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"/*dialect*/create table {tmpTableName}");
            stringBuilder.AppendLine(" ( ");
            for (int i = 0; i < Field.Length; i++)
            {
                if (i == Field.Length - 1)
                {
                    stringBuilder.AppendLine(Field[i] + " " + FieldType[i] + ")");
                }
                else
                {
                    stringBuilder.AppendLine(Field[i] + " " + FieldType[i] + ",");
                }
            }
            return stringBuilder.ToString();
        }

        public void InventoryDetailSummarySql(string temp)
        {
            StringBuilder stringBuilder = new StringBuilder();
            StringBuilder stringBuilder2 = new StringBuilder();
            stringBuilder.AppendLine(" where 1=1 ");
            if (!string.IsNullOrWhiteSpace(F_UJED_BeginDatetime))
            {
                stringBuilder.AppendLine(" and a.fdate>='" + F_UJED_BeginDatetime + "' ");
            }
            if (!string.IsNullOrWhiteSpace(F_UJED_EndDatetime))
            {
                stringBuilder.AppendLine(" and a.fdate<='" + F_UJED_EndDatetime + "' ");
            }
            if (!string.IsNullOrWhiteSpace(F_UJED_Supplier))
            {
                stringBuilder.AppendLine(" and s.FNUMBER='" + F_UJED_Supplier + "' ");
                stringBuilder2.AppendLine(" and s.FNUMBER='" + F_UJED_Supplier + "' ");
            }
            if (!string.IsNullOrWhiteSpace(F_UJED_Material))
            {
                stringBuilder.AppendLine(" and m.fnumber='" + F_UJED_Material + "' ");
                stringBuilder2.AppendLine(" and m.fnumber='" + F_UJED_Material + "' ");
            }
            if (!string.IsNullOrWhiteSpace(F_UJED_OrgId))
            {
                stringBuilder.AppendLine(" and FPURCHASEORGID='" + F_UJED_OrgId + "' ");
                stringBuilder2.Append(" and a.FPURCHASEORGID='" + F_UJED_OrgId + "' ");
            }
            //基准价匹配是否包含供应商
            string isSupplierBB1Where = Fissupplier ? "and a.FSUPPLIERID=BB1.FSUPPLIERID" : "";
            string isSupplierCC1Where = Fissupplier ? "and a.FSUPPLIERID=CC1.FSUPPLIERID" : "";

            StringBuilder stringBuilder3 = new StringBuilder();

            #region BB1 小于基准时间表
            stringBuilder3.AppendLine("/*dialect*/WITH  BB1 AS ( ");
            stringBuilder3.AppendLine(" SELECT b.FMATERIALID, a.FSUPPLIERID, a.FDATE, b.fentryid, a.FBILLNO, b.FSEQ, b.FNOTE, ");
            stringBuilder3.AppendLine(" c.FTAXPRICE, pe.FTAXPRICE FTAXPRICEZG, pce.FTAXPRICE FTAXPRICECW, ");
            stringBuilder3.AppendLine(" p.FBILLNO fbillnoZG, pe.FSEQ fseqZG, pe.FComment FCommentZG, ");
            stringBuilder3.AppendLine(" pC.FBILLNO fbillnoCW, pce.FSEQ fseqCW, pce.FComment FCommentCW, ");
            stringBuilder3.AppendLine(" ROW_NUMBER() OVER(PARTITION BY b.FMATERIALID, a.FSUPPLIERID ORDER BY a.FDATE DESC, b.fentryid ASC ) AS RowNum ");
            stringBuilder3.AppendLine(" FROM t_STK_InStock a ");
            stringBuilder3.AppendLine(" INNER JOIN t_STK_InStockENTRY b ON a.fid = b.FID ");
            stringBuilder3.AppendLine(" INNER JOIN t_STK_InStockENTRY_F c ON b.FENTRYID = c.FENTRYID ");
            stringBuilder3.AppendLine(" left join T_BD_MATERIAL m on m.FMATERIALID=b.FMATERIALID ");
            stringBuilder3.AppendLine(" left join T_BD_SUPPLIER s on s.FSUPPLIERID=a.FSUPPLIERID ");
            stringBuilder3.AppendLine(" LEFT JOIN T_AP_PAYABLE_LK pl ON pl.FSID = b.FENTRYID AND pl.FSTABLENAME = 'T_STK_INSTOCKENTRY' ");
            stringBuilder3.AppendLine(" LEFT JOIN T_AP_PAYABLEENTRY pE ON pE.FENTRYID = pl.FENTRYID ");
            stringBuilder3.AppendLine(" left join T_AP_PAYABLE P ON P.FID = PE.FID ");
            stringBuilder3.AppendLine(" LEFT JOIN T_AP_PAYABLE_LK plc ON plc.FSID = PE.FENTRYID AND plc.FSTABLENAME = 'T_AP_PAYABLEENTRY' ");
            stringBuilder3.AppendLine(" LEFT JOIN T_AP_PAYABLEENTRY pce ON pce.FENTRYID = plc.FENTRYID ");
            stringBuilder3.AppendLine(" left join T_AP_PAYABLE Pc ON Pc.FID = pce.FID ");
            stringBuilder3.AppendLine($" WHERE a.FDATE < '{F_UJED_PriceDatetime}' {stringBuilder2} AND c.FTAXPRICE > 0 )");
            #endregion BB1 小于基准时间表

            #region CC1 大于基准时间表
            stringBuilder3.AppendLine(", CC1 AS ( ");
            stringBuilder3.AppendLine(" SELECT b.FMATERIALID, a.FSUPPLIERID, a.FDATE, b.fentryid, a.FBILLNO, b.FSEQ, b.FNOTE, ");
            stringBuilder3.AppendLine(" c.FTAXPRICE, pe.FTAXPRICE FTAXPRICEZG, pce.FTAXPRICE FTAXPRICECW, ");
            stringBuilder3.AppendLine(" p.FBILLNO fbillnoZG, pe.FSEQ fseqZG, pe.FComment FCommentZG, ");
            stringBuilder3.AppendLine(" pC.FBILLNO fbillnoCW, pce.FSEQ fseqCW, pce.FComment FCommentCW, ");
            stringBuilder3.AppendLine(" ROW_NUMBER() OVER(PARTITION BY b.FMATERIALID, a.FSUPPLIERID ORDER BY a.FDATE,b.fentryid DESC ) AS RowNum ");
            stringBuilder3.AppendLine(" FROM t_STK_InStock a ");
            stringBuilder3.AppendLine(" INNER JOIN t_STK_InStockENTRY b ON a.fid = b.FID ");
            stringBuilder3.AppendLine(" INNER JOIN t_STK_InStockENTRY_F c ON b.FENTRYID = c.FENTRYID ");
            stringBuilder3.AppendLine(" left join T_BD_MATERIAL m on m.FMATERIALID=b.FMATERIALID ");
            stringBuilder3.AppendLine(" left join T_BD_SUPPLIER s on s.FSUPPLIERID=a.FSUPPLIERID ");
            stringBuilder3.AppendLine(" LEFT JOIN T_AP_PAYABLE_LK pl ON pl.FSID = b.FENTRYID AND pl.FSTABLENAME = 'T_STK_INSTOCKENTRY' ");
            stringBuilder3.AppendLine(" LEFT JOIN T_AP_PAYABLEENTRY pE ON pE.FENTRYID = pl.FENTRYID ");
            stringBuilder3.AppendLine(" left join T_AP_PAYABLE P ON P.FID = PE.FID ");
            stringBuilder3.AppendLine(" LEFT JOIN T_AP_PAYABLE_LK plc ON plc.FSID = PE.FENTRYID AND plc.FSTABLENAME = 'T_AP_PAYABLEENTRY' ");
            stringBuilder3.AppendLine(" LEFT JOIN T_AP_PAYABLEENTRY pce ON pce.FENTRYID = plc.FENTRYID ");
            stringBuilder3.AppendLine(" left join T_AP_PAYABLE Pc ON Pc.FID = pce.FID ");
            stringBuilder3.AppendLine($" WHERE a.FDATE > '{F_UJED_PriceDatetime}' {stringBuilder2} AND c.FTAXPRICE > 0 )");
            #endregion CC1 大于基准时间表

            #region 主表
            stringBuilder3.AppendLine(" SELECT  a.fid as orderfid,x.FNAME Fbilltype,a.fbillno,a.fdate,b.FSEQ,b.FNOTE,s.FSUPPLIERID, ");
            stringBuilder3.AppendLine("  s.FNUMBER as FSUPPLIERNumber,sl.fname as FSUPPLIERName,m.FMATERIALID,m.fnumber as FMATERIALNumber,ml.FNAME as FMATERIALName, ");
            stringBuilder3.AppendLine(" u.Fname FuserName,isnull(d.FNAME,'') fdeptname, ");
            stringBuilder3.AppendLine(" FREQUIRESTAFFID,a.F_QCPR_PROJECT, ");
            stringBuilder3.AppendLine(" FUNITID,FSETTLECURRID,FLOCALCURRID,FALLAMOUNT,FALLAMOUNT_LC,FAMOUNT,FAMOUNT_LC, ");
            stringBuilder3.AppendLine(" isnull(mg1l.FNAME,'') Fmg1lFNAME,isnull(mg2l.FNAME,'') Fmg2lFNAME,isnull(mg3l.FNAME,'') Fmg3lFNAME, ");
            stringBuilder3.AppendLine(" case when FGIVEAWAY=1 then '是' else '否' end  FGIVEAWAY, ");
            stringBuilder3.AppendLine(" ol.FNAME FPURCHASEORGName, ");
            stringBuilder3.AppendLine(" isnull(pay.FNAME,'') Fpayname, ");
            stringBuilder3.AppendLine(" fol.FCAPTION FDOCUMENTSTATUS, ");
            stringBuilder3.AppendLine(" FREALQTY,FTaxRate,FPRICE,c.FTAXPRICE, ");
            stringBuilder3.AppendLine(" COALESCE(BB1.FTAXPRICE, CC1.FTAXPRICE, c.FTAXPRICE) AS FTAXPRICE2, ");
            stringBuilder3.AppendLine(" COALESCE(BB1.FTAXPRICEZG, CC1.FTAXPRICEZG, c.FTAXPRICE) AS FTAXPRICE3, ");
            stringBuilder3.AppendLine(" COALESCE(BB1.FTAXPRICECW, CC1.FTAXPRICECW, c.FTAXPRICE) AS FTAXPRICE4, ");
            stringBuilder3.AppendLine(" c.FTAXPRICE-COALESCE(BB1.FTAXPRICE, CC1.FTAXPRICE, c.FTAXPRICE) Fpricediff, ");
            stringBuilder3.AppendLine(" COALESCE(BB1.FBILLNO, CC1.FBILLNO, '') FbaseBillno, ");
            stringBuilder3.AppendLine(" COALESCE(BB1.FNote, CC1.FNote, '') FbaseNote, ");
            stringBuilder3.AppendLine(" COALESCE(CAST(BB1.FSEQ AS VARCHAR(10)),CAST(CC1.FSEQ AS VARCHAR(10)), '') FbaseSeq, ");
            stringBuilder3.AppendLine(" COALESCE(BB1.fbillnoZG, CC1.fbillnoZG, '') fbillnoZG, ");
            stringBuilder3.AppendLine(" COALESCE(BB1.FCommentZG, CC1.FCommentZG, '') FCommentZG, ");
            stringBuilder3.AppendLine(" COALESCE(CAST(BB1.fseqZG AS VARCHAR(10)),CAST(CC1.fseqZG AS VARCHAR(10)), '') fseqZG, ");
            stringBuilder3.AppendLine(" COALESCE(BB1.fbillnoCW, CC1.fbillnoCW, '') fbillnoCW, ");
            stringBuilder3.AppendLine(" COALESCE(BB1.FCommentCW, CC1.FCommentCW, '') FCommentCW, ");
            stringBuilder3.AppendLine(" COALESCE(CAST(BB1.fseqCW AS VARCHAR(10)),CAST(CC1.fseqCW AS VARCHAR(10)), '') fseqCW ");
            stringBuilder3.AppendLine(" ,a.F_QCPR_Base Fproductline ");
            stringBuilder3.AppendLine(" from t_STK_InStock a  inner join T_PUR_POORDERFIN F on a.fid=F.FID ");
            stringBuilder3.AppendLine(" inner join t_STK_InStockENTRY b on a.fid=b.FID ");
            stringBuilder3.AppendLine(" inner join t_STK_InStockENTRY_F c on b.FENTRYID=c.FENTRYID ");
            stringBuilder3.AppendLine(" left join T_BD_SUPPLIER s on s.FSUPPLIERID=a.FSUPPLIERID ");
            stringBuilder3.AppendLine(" left join T_BD_SUPPLIER_L sl on s.FSUPPLIERID=sl.FSUPPLIERID   and sl.FLOCALEID=2052 ");
            stringBuilder3.AppendLine(" left join T_BD_MATERIAL m on m.FMATERIALID=b.FMATERIALID ");
            stringBuilder3.AppendLine(" left join T_BD_MATERIAL_L ml on m.FMATERIALID=ml.FMATERIALID   and ml.FLOCALEID=2052 ");
            stringBuilder3.AppendLine(" left join T_ORG_ORGANIZATIONS o on o.FORGID= a.FPURCHASEORGID ");
            stringBuilder3.AppendLine(" left join T_ORG_ORGANIZATIONS_L ol on o.FORGID= ol.FORGID    and ol.FLOCALEID=2052   ");
            stringBuilder3.AppendLine(" left join T_BAS_BILLTYPE_L x on x.FBILLTYPEID= a.FBILLTYPEID ");
            stringBuilder3.AppendLine(" left join T_SEC_USER u on u.FUSERID=a.FCREATORID ");
            stringBuilder3.AppendLine(" left join T_BD_DEPARTMENT_L d on d.FDEPTID=FPURCHASEDEPTID ");
            stringBuilder3.AppendLine(" left join T_BD_MATERIALGROUP mg1 on mg1.FID=m.FMATERIALGROUP left join T_BD_MATERIALGROUP_L mg1l on mg1l.FID=mg1.fid ");
            stringBuilder3.AppendLine(" left join T_BD_MATERIALGROUP mg2 on mg2.FID=mg1.FPARENTID left join T_BD_MATERIALGROUP_L mg2l on mg2l.FID=mg2.fid ");
            stringBuilder3.AppendLine(" left join T_BD_MATERIALGROUP mg3 on mg3.FID=mg2.FPARENTID left join T_BD_MATERIALGROUP_L mg3l on mg3l.FID=mg3.fid ");
            stringBuilder3.AppendLine($" LEFT JOIN (SELECT * FROM BB1 WHERE RowNum=1) BB1 ON b.FMATERIALID = BB1.FMATERIALID {isSupplierBB1Where} ");
            stringBuilder3.AppendLine($" LEFT JOIN (SELECT * FROM CC1 WHERE RowNum=1) CC1 ON b.FMATERIALID = CC1.FMATERIALID {isSupplierCC1Where} ");
            stringBuilder3.AppendLine(" left join T_BD_PAYMENTCONDITION_L pay on pay.FID=f.FPAYCONDITIONID ");
            stringBuilder3.AppendLine(" left join T_META_FORMENUMITEM fo on  fo.fid='14039efd-6350-4eab-b482-c1c6bcdf914b' and fo.FVALUE= a.FDOCUMENTSTATUS ");
            stringBuilder3.AppendLine(" left join T_META_FORMENUMITEM_L fol on fo.FENUMID=fol.FENUMID  ");
            stringBuilder3.AppendLine($"{stringBuilder}");
            #endregion 主表

            Logger logger = new Logger(Logpath, DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
            logger.WriteLog("执行sql：" + stringBuilder3.ToString());

            #region 插入临时表
            StringBuilder stringBuilder4 = new StringBuilder();
            stringBuilder4.AppendLine("/*dialect*/");
            DynamicObjectCollection val = DBUtils.ExecuteDynamicObject(((AbstractSysReportServicePlugIn)this).Context, string.Format(stringBuilder3.ToString()), (IDataEntityType)null, (IDictionary<string, Type>)null, CommandType.Text, Array.Empty<SqlParam>());
            int num = 1;
            foreach (dynamic item in (Collection<DynamicObject>)(object)val)
            {
                stringBuilder4.AppendFormat("Insert into {0}  ", temp);
                stringBuilder4.Append(" ( ");
                for (int i = 0; i < Field.Length; i++)
                {
                    if (i == Field.Length - 1)
                    {
                        stringBuilder4.Append(Field[i] + ")");
                    }
                    else
                    {
                        stringBuilder4.Append(Field[i] + ",");
                    }
                }
                stringBuilder4.Append(" values ");
                stringBuilder4.Append("(");
                for (int j = 0; j < Field.Length; j++)
                {
                    if (j == 0)
                    {
                        stringBuilder4.Append(num++ + ",");
                    }
                    else if (j == Field.Length - 1)
                    {
                        stringBuilder4.Append("'" + item[Field[j]].ToString().Replace("'", "''") + "'");
                    }
                    else
                    {
                        stringBuilder4.Append("'" + item[Field[j]].ToString().Replace("'", "''") + "',");
                    }
                }
                stringBuilder4.AppendLine(")");
            }
            DBUtils.Execute(((AbstractSysReportServicePlugIn)this).Context, stringBuilder4.ToString());
            #endregion 插入临时表

        }

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
        public override ReportHeader GetReportHeaders(IRptParams filter)
        {

            ReportHeader val = new ReportHeader();
            for (int i = 1; i < Field1.Length; i++)
            {
                ((ListHeader)val).AddChild(Field[i], new LocaleValue(Field1[i])).ColIndex = i;
            }
            return val;
        }

        private void FilterParameter(IRptParams filter)
        {
            DynamicObject customFilter = filter.FilterParameter.CustomFilter;
            if (customFilter != null)
            {
                F_UJED_BeginDatetime = ((customFilter["F_UJED_BeginDatetime"] != null) ? customFilter["F_UJED_BeginDatetime"].ToString() : "");
                F_UJED_EndDatetime = ((customFilter["F_UJED_EndDatetime"] != null) ? customFilter["F_UJED_EndDatetime"].ToString() : "");
                F_UJED_PriceDatetime = ((customFilter["F_UJED_PriceDatetime"] != null) ? customFilter["F_UJED_PriceDatetime"].ToString() : "");
                F_UJED_Supplier = ((customFilter["F_UJED_Supplier"] != null) ? ((DynamicObject)customFilter["F_UJED_Supplier"])["Number"].ToString() : "");
                F_UJED_Material = ((customFilter["F_UJED_Material"] != null) ? ((DynamicObject)customFilter["F_UJED_Material"])["Number"].ToString() : "");
                F_UJED_OrgId = ((customFilter["F_UJED_OrgId"] != null) ? ((DynamicObject)customFilter["F_UJED_OrgId"])["ID"].ToString() : "");
                F_UJED_OrgName = ((customFilter["F_UJED_OrgId"] != null) ? ((DynamicObject)customFilter["F_UJED_OrgId"])["Name"].ToString() : "");
                Fissupplier = Convert.ToBoolean(customFilter["Fissupplier"]);
            }
        }

        public override ReportTitles GetReportTitles(IRptParams filter)
        {
            //IL_0001: Unknown result type (might be due to invalid IL or missing references)
            //IL_0007: Expected O, but got Unknown
            ReportTitles val = new ReportTitles();
            val.AddTitle("F_UJED_BeginDatetime", F_UJED_BeginDatetime);
            val.AddTitle("F_UJED_EndDatetime", F_UJED_EndDatetime);
            val.AddTitle("F_UJED_PriceDatetime", F_UJED_PriceDatetime);
            val.AddTitle("F_UJED_Supplier", F_UJED_Supplier);
            val.AddTitle("F_UJED_Material", F_UJED_Material);
            val.AddTitle("F_UJED_OrgId", F_UJED_OrgName);
            return val;
        }
    }
}
