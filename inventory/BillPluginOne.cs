

using Kingdee.BOS.Core.CommonFilter;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.JSON;
using Kingdee.K3.SCM.Stock.Report.PlugIn;
using System;
using System.ComponentModel;

namespace BD.Standard.KY.ReportServicePlugIn
{
    [Kingdee.BOS.Util.HotUpdate]
    [Description("库龄分析明细表单扩展插件")]
    public class BillPluginOne : InvAgeDetailFilter
    {

        public override void TreeNodeClick(TreeNodeArgs e)
        {
            base.TreeNodeClick(e);

            ICommonFilterModelService model = this.Model as ICommonFilterModelService;
            var showHideCtrl = this.View.GetControl<FieldShowHide>("FFieldShowHideSet");
            Kingdee.BOS.JSON.JSONArray ShowHideRows = model.ColumnObject.GetShowHideRows();
            foreach (JSONObject row in ShowHideRows)
            {
                row["DefaultVisible"] = true;
                row["Visible"] = true;
                Console.WriteLine();
            }
            showHideCtrl.SetShowHideRows(ShowHideRows);

        }

    }
}
