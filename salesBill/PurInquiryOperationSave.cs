using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace BD.Standard.KY.ReportServicePlugIn
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("销售订单保存操作插件")]
    public class PurInquiryOperationSave : AbstractOperationServicePlugIn
    {
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            IOperationResult operationResult = new OperationResult();
            try
            {
                foreach (DynamicObject entity in e.DataEntitys)
                {
                    string fid = entity[0].ToString();
                    string FCustId = ((DynamicObject)entity["CustId"])[0].ToString();
                    DynamicObjectCollection dyc = entity["SaleOrderEntry"] as DynamicObjectCollection;
                    foreach (DynamicObject item in dyc)
                    {
                        string FMaterialId=((DynamicObject)item["MaterialId"])[0].ToString();
                        string FMaterialNumber = ((DynamicObject)item["MaterialId"])["Number"].ToString();
                        string FSeq = item["Seq"].ToString();
                        string FTaxPrice = item["TaxPrice"].ToString();
                        string sql = $"select sum(FQTY) Fsumqty from T_SAL_ORDER a inner join T_SAL_ORDERENTRY b on a.fid=b.fid where FMaterialId ={FMaterialId} and FCustId={FCustId} group by  fCustId,fMaterialId";
                        decimal Fsumqty = DBUtils.ExecuteScalar<decimal>(this.Context, sql, 0, null);

                        sql = $"select FPrice  from  T_SAL_APPLYCUSTOMER a  inner join T_SAL_PRICELISTENTRY b on a.fid=b.fid  where FMaterialId={FMaterialId} and FCustId={FCustId} and FFROMQTY<{Fsumqty} and FTOQTY>{Fsumqty} and FEFFECTIVEDATE<'{DateTime.Now.ToString()}' and FEXPRIYDATE>'{DateTime.Now.ToString()}'";
                        DynamicObjectCollection dy = DBUtils.ExecuteDynamicObject(Context, sql);


                        string result = "";
                        if (dy.Count==1)
                        {
                            if (Convert.ToDecimal(dy[0]["FPrice"].ToString()) != Convert.ToDecimal(FTaxPrice))
                            {
                                result = $"第{FSeq}行物料{FMaterialNumber}历史销售订单数量为{Fsumqty.ToString("0.##########")}，阶梯价格为{Convert.ToDecimal(dy[0]["FPrice"].ToString()).ToString("0.##########")}，当前价格为{Convert.ToDecimal(FTaxPrice).ToString("0.##########")}，有差异，请及时调整当前价格";
                            }
                            else
                            {
                                result = "无差异";
                                item["F_result"] = result;
                                DBUtils.Execute(Context, $"UPDATE T_SAL_ORDERENTRY SET F_result='{result}' where fentryid={item["id"]}");
                                continue;
                            }
                        }
                        else if(dy.Count > 1)
                        {
                            result = $"第{FSeq}行物料{FMaterialNumber}历史销售订单数量为{Fsumqty.ToString("0.##########")},价目表阶梯数量存在交叉，请检查销售价目表";
                        }
                        else
                        {
                            result = $"第{FSeq}行物料{FMaterialNumber}历史销售订单数量为{Fsumqty.ToString("0.##########")},价目表未录入当前物料或价目表阶梯数量未匹配，请检查销售价目表";
                        }

                        //第x行物料xxxx历史销售订单数量为yyy，阶梯价格为y，当前价格为yy，有差异，请及时调整当前价格”
                        //“第x行物料xxxx历史销售订单数量为yyy，价目表阶梯数量未匹配，请录入价目表信息”
                        //“价目表未录入当前物料或价目表阶梯数量存在交叉，请检查销售价目表”

                        operationResult.OperateResult.Add(new OperateResult()
                        {
                            SuccessStatus = true,
                            Name = "匹配价目表",
                            Message = string.Format(result),
                            MessageType = MessageType.Normal,
                            PKValue = 0,
                        });
                        item["F_result"] = result;
                        DBUtils.Execute(Context, $"UPDATE T_SAL_ORDERENTRY SET F_result='{result}' where fentryid={item["id"]}");

                    }

                }
                this.OperationResult.MergeResult(operationResult);
                
            }
            catch (Exception ex)
            {
                throw new KDException("",ex.Message);
            }
            
        }


    
    }
}
