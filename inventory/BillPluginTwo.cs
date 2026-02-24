
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml;

namespace BD.Standard.KY.ReportServicePlugIn
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("库龄分析明细二开表单插件")]
    public class BillPluginTwo : AbstractDynamicFormPlugIn
    {
        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);

            ComboFieldEditor headComboEidtor = this.View.GetControl<ComboFieldEditor>("Fscheme");
            List<EnumItem> comboOptions = new List<EnumItem>();
            string sql = "exec KY_ReportFscheme";
            DynamicObjectCollection dy = DBUtils.ExecuteDynamicObject(this.Context, sql);
            if (dy != null && dy.Count > 0)
            {
                int count = 1;
                comboOptions.Add(new EnumItem() { EnumId = "", Value = "", Caption = new LocaleValue("") });
                foreach (DynamicObject item in dy)
                {
                    comboOptions.Add(new EnumItem() { EnumId = Convert.ToString(count++), Value = item["FSCHEMEID"].ToString(), Caption = new LocaleValue(item["FSCHEMENAME"].ToString()) });





                }
                headComboEidtor.SetComboItems(comboOptions);
            }

            string Fscheme = this.View.Model.GetValue("Fscheme").ToString();
            if (!string.IsNullOrWhiteSpace(Fscheme))
            {
                DynamicObjectCollection dy1 = DBUtils.ExecuteDynamicObject(this.Context, $"SELECT FSCHEME FROM T_BAS_FILTERSCHEME WHERE FSCHEMEID='{Fscheme}'");
                string FSCHEME = dy1[0]["FSCHEME"].ToString();

                JObject jObject = GetFilter(FSCHEME);
                string Filter = jObject["SchemeEntity"]["CustomFilterSetting"].ToString();
                JObject Filterjson = GetFilter(Filter);
                JObject DetailFilter = (JObject)Filterjson["InvAgeDetailFilter"];
                DynamicObjectCollection dys = DBUtils.ExecuteDynamicObject(this.Context, $"select FSPECIFYDATE from t_stk_invagetask where FSCHEMEID ={DetailFilter["SchemeId_Id"].ToString()}");
                string FQueryDate = dys[0]["FSPECIFYDATE"].ToString().Contains("0001-01-01") ? DateTime.Now.ToString() : dys[0]["FSPECIFYDATE"].ToString();
                this.View.Model.SetValue("FQueryDate", FQueryDate);
            }
            



            this.View.Model.SetValue("FRadioGroup", "A");

            InitAgeEntity();
        }


        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            if (e.Field.Key.ToUpperInvariant().Equals("FINTERVALDAY"))
            {
                int row = e.Row;
                string format = ResManager.LoadKDString("{0}天到{1}天", "004024030002392", SubSystemType.SCM);
                int num = 0;
                Entity entity = View.BusinessInfo.GetEntity("FInvAgeEntity");
                DynamicObjectCollection entityDataObject = Model.GetEntityDataObject(entity);
                List<DynamicObject> list = new List<DynamicObject>(entityDataObject);
                entityDataObject.Clear();
                foreach (DynamicObject item in list)
                {
                    if (Convert.ToInt32(item["IntervalDay"]) > 0)
                    {
                        item["DownDay"] = num;
                        num += Convert.ToInt32(item["IntervalDay"]);
                        item["UpperDay"] = num - 1;
                        item["IntervalTxt"] = string.Format(format, item["DownDay"], item["UpperDay"]);
                        entityDataObject.Add(item);
                    }
                }

                DynamicObject dynamicObject = new DynamicObject(entityDataObject.DynamicCollectionItemPropertyType);
                dynamicObject["IntervalTxt"] = string.Format(ResManager.LoadKDString("{0}天以上", "004024030002395", SubSystemType.SCM), num);
                dynamicObject["DownDay"] = num;
                dynamicObject["UpperDay"] = 0;
                entityDataObject.Add(dynamicObject);
                View.UpdateView("FInvAgeEntity");
                View.SetEntityFocusRow("FInvAgeEntity", row + 1);
                if (entityDataObject.Count == 1)
                {
                    InitAgeEntity(1);
                }

            }
            if (e.Field.Key.ToUpperInvariant().Equals("FSCHEME"))
            {
                string newValue = e.NewValue.ToString();
                string sql = $"SELECT FSCHEME FROM T_BAS_FILTERSCHEME WHERE FSCHEMEID='{newValue}'";
                DynamicObjectCollection dy = DBUtils.ExecuteDynamicObject(this.Context, sql);
                string FSCHEME = dy[0]["FSCHEME"].ToString();

                JObject jObject = GetFilter(FSCHEME);
                string Filter = jObject["SchemeEntity"]["CustomFilterSetting"].ToString();
                JObject Filterjson = GetFilter(Filter);
                JObject DetailFilter = (JObject)Filterjson["InvAgeDetailFilter"];


                DynamicObjectCollection dys = DBUtils.ExecuteDynamicObject(this.Context, $"select FSPECIFYDATE from t_stk_invagetask where FSCHEMEID ={DetailFilter["SchemeId_Id"].ToString()}");

                string FQueryDate=dys[0]["FSPECIFYDATE"].ToString().Contains("0001-01-01") ? DateTime.Now.ToString() : dys[0]["FSPECIFYDATE"].ToString();

                  this.View.Model.SetValue("FQueryDate", FQueryDate);

                string FMaterialFrom = DetailFilter.TryGetValue("MaterialFrom_Id", out var MaterialFrom_Id) ? MaterialFrom_Id.ToString() : "0";
                this.View.Model.SetValue("FMaterialFrom", FMaterialFrom);
                string FMaterialTo = DetailFilter.TryGetValue("MaterialTo_Id", out var MaterialTo_Id) ? MaterialTo_Id.ToString() : "0";
                this.View.Model.SetValue("FMaterialTo", FMaterialTo);
                string FStockFrom = DetailFilter.TryGetValue("StockFrom_Id", out var StockFrom_Id) ? StockFrom_Id.ToString() : "0";
                this.View.Model.SetValue("FStockFrom", FStockFrom);
                string FStockTo = DetailFilter.TryGetValue("StockTo_Id", out var StockTo_Id) ? StockTo_Id.ToString() : "0";
                this.View.Model.SetValue("FStockTo", FStockTo);
                string FLotFrom = DetailFilter.TryGetValue("LotFrom_Id", out var LotFrom_Id) ? LotFrom_Id.ToString() : "0";
                this.View.Model.SetValue("FLotFrom", FLotFrom);
                string FLotTo = DetailFilter.TryGetValue("LotTo_Id", out var LotTo_Id) ? LotTo_Id.ToString() : "0";
                this.View.Model.SetValue("FLotTo", FLotTo);
                string FPageByOrg = DetailFilter.TryGetValue("PageByOrg", out var PageByOrg) ? PageByOrg.ToString() : "False";
                this.View.Model.SetValue("FPageByOrg", FPageByOrg);
                string FPageByOwner = DetailFilter.TryGetValue("PageByOwner", out var PageByOwner) ? PageByOwner.ToString() : "False";
                this.View.Model.SetValue("FPageByOwner", FPageByOwner);
                string FShowForbidMaterial = DetailFilter.TryGetValue("ShowForbidMaterial", out var ShowForbidMaterial) ? ShowForbidMaterial.ToString() : "False";
                this.View.Model.SetValue("FShowForbidMaterial", FShowForbidMaterial);
                string FCurDateIsOne = DetailFilter.TryGetValue("CurDateIsOne", out var CurDateIsOne) ? CurDateIsOne.ToString() : "False";
                this.View.Model.SetValue("FCurDateIsOne", FCurDateIsOne);
            }
        }

        private static JObject GetFilter(string FSCHEME)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(FSCHEME);
            // 转换为JSON
            string json = JsonConvert.SerializeXmlNode(doc);
            JObject jObject = JObject.Parse(json);
            return jObject;
        }

        private void InitAgeEntity(int day = 15)
        {
            View.Model.SetValue("FINTERVALDAY", day, 0);
        }

        public override void ButtonClick(ButtonClickEventArgs e)
        {
            DynamicObjectCollection configs = DBUtils.ExecuteDynamicObject(this.Context, "exec KY_ReportConfig");
            Kingdee.BOS.WebApi.Client.K3CloudApiClient k3CloudApiClient = HttpPost.WebApiClent(configs[0]["url"].ToString(), configs[0]["dbid"].ToString(), configs[0]["name"].ToString(), configs[0]["pwd"].ToString());
            NewMethod(k3CloudApiClient, 0);

            base.ButtonClick(e);
        }

        public void NewMethod(Kingdee.BOS.WebApi.Client.K3CloudApiClient k3CloudApiClient, int row)
        {
            string requestUrl = "Kingdee.K3.SCM.WebApi.ServicesStub.StockReportQueryService.GetReportData";
            JArray jArray = new JArray()
            {
                new JObject()
                {
                    {"FORMID","STK_InvAgeDetailRpt"},
                    {"FSCHEMEID",this.Model.GetValue("Fscheme").ToString()},
                    {"StartRow",10000*row},
                    {"Limit",10000},
                }
            };

            //自定义请求参数
            object[] paramInfo = JsonConvert.DeserializeObject<object[]>(jArray.ToString());
            var resultJson = k3CloudApiClient.Execute<string>(requestUrl, paramInfo);
            var resultJObject = JObject.Parse(resultJson);
            if (Convert.ToBoolean(resultJObject["success"].ToString()))
            {
                JToken jToken = resultJObject["data"];
                if (jToken == null || jToken.Type == JTokenType.Null)
                {
                    throw new Exception("执行方案未查询到数据！");
                }

                JArray datas = JArray.Parse(resultJObject["data"].ToString());
                foreach (JProperty property in ((JObject)datas[0]).Properties())
                {
                    string key = property.Name;
                    string checkColumnSql = $@"
                            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                                           WHERE TABLE_NAME = 'KY_GetReportDataTable' AND COLUMN_NAME = '{key}')
                            BEGIN
                                ALTER TABLE KY_GetReportDataTable  ADD {key} NVARCHAR(255) NULL
                            END";
                    DBUtils.Execute(this.Context, checkColumnSql);
                }

                foreach (JObject data in datas)
                {
                    StringBuilder sb = new StringBuilder();
                    StringBuilder column = new StringBuilder();
                    StringBuilder values = new StringBuilder();
                    sb.Append(string.Format(@"/*dialect*/  insert into KY_GetReportDataTable "));
                    values.Append("  values (");
                    column.Append("(");
                    foreach (JProperty property in data.Properties())
                    {
                        string key = property.Name;
                        string value = property.Value.ToString();
                        column.Append(key + ",");
                        values.Append("'" + value.Replace("'", "''") + "',");
                    }
                    column.Remove(column.Length - 1, 1);
                    values.Remove(values.Length - 1, 1);
                    column.Append(" )");
                    values.Append(" )");
                    DBUtils.Execute(Context, sb.Append(column.ToString()).Append(values.ToString()).ToString());

                }

                int totalCount = Convert.ToInt32(resultJObject["TotalCount"].ToString());
                int pageNos = Convert.ToInt32(totalCount / 10000 + (totalCount % 10000 > 0 ? 1 : 0));
                if (++row < pageNos)
                {
                    NewMethod(k3CloudApiClient, row);
                }

            }
            else
            {
                throw new Exception(resultJObject["message"].ToString());
            }
        }
    }
}
