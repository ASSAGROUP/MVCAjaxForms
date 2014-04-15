using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using OpencutBLL;
using OpencutDAL.DO.OMSPRD;


namespace System.Web.Helpers
{
    public static class HtmlHelpers
    {



        /// <summary>
        /// Ensures a consistant phone number format regardless of original format
        /// </summary>
        /// <param name="html"></param>
        /// <param name="phoneNumber"></param>
        /// <returns></returns>
        public static MvcHtmlString FormatPhoneNumber(this HtmlHelper html, string phoneNumber)
        {
            try
            {
                // remove any non-digit characters
                string number = Regex.Replace(phoneNumber, @"[^\d]", "");

                // return blank if no number recorded
                if (number.Length == 0)
                    return new MvcHtmlString("");

                // trim off the extension
                string ext = string.Empty;
                if (number.Length > 10)
                {
                    ext = " X " + number.Substring(10);
                    number = number.Substring(0, 10);
                }


                // area code
                string areaCode = number.Length == 10 ? number.Substring(0, 3) : "406";

                // prefix
                string prefix = string.Empty;
                if (number.Length == 10)
                    prefix = number.Substring(3, 3);
                else if (number.Length == 7)
                    prefix = number.Substring(0, 3);

                // suffix
                string suffix = string.Empty;
                if (number.Length == 10)
                    suffix = number.Substring(6, 4);
                else if (number.Length == 7)
                    suffix = number.Substring(3, 4);

                // combine into formatted phone number
                string format = string.Format("({0}) {1}-{2}{3}", areaCode, prefix, suffix, ext);


                return new MvcHtmlString(format);

            }
            catch (Exception ex)
            {
                string message = string.Format("<span title='{0}'>Invalid Phone #</span>", ex.Message);
                return new MvcHtmlString(message);
            }
        }



        #region Dropdown List Helpers

        /// <summary>
        /// Converts a list of strings into an list of SelectListItems where the text and value of each item are identical
        /// </summary>
        /// <param name="html"></param>
        /// <param name="Values"></param>
        /// <param name="SelectedValue"></param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> SelectListItems(this HtmlHelper html, IEnumerable<string> Values, string SelectedValue)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (string value in Values)
            {
                SelectListItem item = new SelectListItem() { Text = value, Value = value, Selected = value == SelectedValue };
                list.Add(item);
            }

            return list;
        }


        /// <summary>
        /// Creates a dropdown list with items separated into Option Groups
        /// </summary>
        /// <param name="html"></param>
        /// <param name="OptGroups"></param>
        /// <param name="DefaultValue"></param>
        /// <param name="IsRequired"></param>
        /// <param name="SelectedValue"></param>
        /// <param name="Name"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        public static MvcHtmlString DropDownOptListFor(this HtmlHelper html, IEnumerable<DropDownListOptGroup> OptGroups, string DefaultValue, bool IsRequired, string SelectedValue, string Name, object htmlAttributes)
        {
            if (string.IsNullOrEmpty(SelectedValue))
                SelectedValue = "";

            string required = "";
            if (IsRequired)
                required = string.Format("data-val=\"true\" data-val-required=\"You must select a {0}\"", Name);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<select name=\"{0}\" id=\"{0}\"{1}>\n", Name, required);
            sb.AppendFormat("<option value=\"\">{0}</option>\n", DefaultValue);

            foreach (DropDownListOptGroup group in OptGroups)
            {
                sb.AppendFormat("<optgroup label=\"{0}\" />\n", group.OptGroupLable);
                foreach (SelectListItem item in group.Items)
                {
                    sb.AppendFormat("<option value=\"{0}\"{1}>{2}</option>\n", item.Value, item.Value == SelectedValue ? " selected=\"selected\"" : "", item.Text);
                }
            }

            sb.Append("</select>");

            return new MvcHtmlString(sb.ToString());
        }


        /// <summary>
        /// The data object used in the DropDownOptListFor helper
        /// </summary>
        public class DropDownListOptGroup
        {
            public IEnumerable<SelectListItem> Items { get; set; }
            public string OptGroupLable { get; set; }
        }

        #endregion


        #region Ajax Dialog Form Helpers


        /// <summary>
        /// Creates a span element with the id of the property and the value as text
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="html"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static MvcHtmlString IDTextFor<TModel, TProperty>(this HtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> ex)
        {
            var metadata = ModelMetadata.FromLambdaExpression(ex, html.ViewData);
            string value = metadata.Model == null ? "" : metadata.Model.ToString();
            string span = string.Format("<span id=\"{0}\">{1}</span>", metadata.PropertyName, value);
            return new MvcHtmlString(span);
        }


        /// <summary>
        /// Creates a button for opening a form in a dialog and using ajax to post data back to server with save and cancel options (no delete)
        /// </summary>
        /// <param name="html">The Html Helper </param>
        /// <param name="buttonText">The text that will be displayed on the button that opens the dialog</param>
        /// <param name="dialogTitle">The title displayed at the top of the dialog</param>
        /// <param name="dialogFormUrl">The url to the action that returns a form within a partial view</param>
        /// <param name="onSaveCallback">The javascript function called after a successful save event</param>
        /// <returns></returns>
        public static MvcHtmlString AjaxDialogFormButton(this HtmlHelper html, string buttonText, string dialogTitle, string dialogFormUrl, string onSaveCallback)
        {
            return AjaxDialogFormButton(html, buttonText, dialogTitle, dialogFormUrl, onSaveCallback, null, null);
        }


        /// <summary>
        /// Creates a button for opening a form in a dialog and using ajax to post data back to server with save, cancel, and delete options
        /// </summary>
        /// <param name="html">The Html Helper </param>
        /// <param name="buttonText">The text that will be displayed on the button that opens the dialog</param>
        /// <param name="dialogTitle">The title displayed at the top of the dialog</param>
        /// <param name="dialogFormUrl">The url to the action that returns a form within a partial view</param>
        /// <param name="onSaveCallback">The javascript function called after a successful save event</param>
        /// <param name="deleteUrl">The url to the action that deletes the record and returns a AjaxResult object</param>
        /// <param name="onDeleteCallback">The javascript function called after a successful delete event</param>
        /// <returns></returns>
        public static MvcHtmlString AjaxDialogFormButton(this HtmlHelper html, string buttonText, string dialogTitle, string dialogFormUrl, string onSaveCallback, string deleteUrl, string onDeleteCallback)
        {
            return AjaxDialogForm(html, buttonText, dialogTitle, dialogFormUrl, onSaveCallback, deleteUrl, onDeleteCallback, AjaxDialogFormTypes.Button);
        }


        /// <summary>
        /// Creates a link for opening a form in a dialog and using ajax to post data back to server with save and cancel options (no delete)
        /// </summary>
        /// <param name="html">The Html Helper </param>
        /// <param name="buttonText">The text that will be displayed on the button that opens the dialog</param>
        /// <param name="dialogTitle">The title displayed at the top of the dialog</param>
        /// <param name="dialogFormUrl">The url to the action that returns a form within a partial view</param>
        /// <param name="onSaveCallback">The javascript function called after a successful save event</param>
        /// <returns></returns>
        public static MvcHtmlString AjaxDialogFormLink(this HtmlHelper html, string linkText, string dialogTitle, string dialogFormUrl, string onSaveCallback)
        {
            return AjaxDialogForm(html, linkText, dialogTitle, dialogFormUrl, onSaveCallback, null, null, AjaxDialogFormTypes.Link);
        }


        /// <summary>
        /// Creates a link for opening a form in a dialog and using ajax to post data back to server with save, cancel, and delete options
        /// </summary>
        /// <param name="html">The Html Helper </param>
        /// <param name="buttonText">The text that will be displayed on the button that opens the dialog</param>
        /// <param name="dialogTitle">The title displayed at the top of the dialog</param>
        /// <param name="dialogFormUrl">The url to the action that returns a form within a partial view</param>
        /// <param name="onSaveCallback">The javascript function called after a successful save event</param>
        /// <param name="deleteUrl">The url to the action that deletes the record and returns a AjaxResult object</param>
        /// <param name="onDeleteCallback">The javascript function called after a successful delete event</param>
        /// <returns></returns>
        public static MvcHtmlString AjaxDialogFormLink(this HtmlHelper html, string linkText, string dialogTitle, string dialogFormUrl, string onSaveCallback, string deleteUrl, string onDeleteCallback)
        {
            return AjaxDialogForm(html, linkText, dialogTitle, dialogFormUrl, onSaveCallback, deleteUrl, onDeleteCallback, AjaxDialogFormTypes.Link);
        }

        
        /// <summary>
        /// The available types of Ajax Dialog Form openers 
        /// </summary>
        private enum AjaxDialogFormTypes
        {
            Button,
            Link
        }


        /// <summary>
        /// Creates an opener for a form in a dialog and using ajax to post data back to server with save, cancel, [and delete] options
        /// </summary>
        /// <param name="html">The Html Helper </param>
        /// <param name="buttonText">The text that will be displayed on the button that opens the dialog</param>
        /// <param name="dialogTitle">The title displayed at the top of the dialog</param>
        /// <param name="dialogFormUrl">The url to the action that returns a form within a partial view</param>
        /// <param name="onSaveCallback">The javascript function called after a successful save event</param>
        /// <param name="deleteUrl">The url to the action that deletes the record and returns a AjaxResult object</param>
        /// <param name="onDeleteCallback">The javascript function called after a successful delete event</param>
        /// <param name="type">The type of dialog opener</param>
        /// <returns></returns>
        private static MvcHtmlString AjaxDialogForm(this HtmlHelper html, string displayText,  string dialogTitle, string dialogFormUrl, string onSaveCallback, string deleteUrl, string onDeleteCallback, AjaxDialogFormTypes type)
        {
            string anchor = "";
            string _class = type == AjaxDialogFormTypes.Button ? "class=\"button\" " : " class=\"\"";

            if (string.IsNullOrEmpty(deleteUrl))
            {
                string anchorFormat = "<a href=\"javascript:void(0);\" data-modal-form=\"true\" data-buttons=\"SaveCancel\" data-title=\"{0}\" data-url=\"{1}\" data-on-save-callback=\"{2}\" {3}>{4}</a>";
                anchor = string.Format(anchorFormat, dialogTitle, dialogFormUrl, onSaveCallback, _class, displayText);
            }
            else
            {
                string anchorFormat = "<a href=\"javascript:void(0);\" data-modal-form=\"true\" data-buttons=\"SaveCancelDelete\" data-title=\"{0}\" data-url=\"{1}\" data-on-save-callback=\"{2}\" data-delete-url=\"{3}\" data-on-delete-callback=\"{4}\" {5}>{6}</a>";
                anchor = string.Format(anchorFormat, dialogTitle, dialogFormUrl, onSaveCallback, deleteUrl, onDeleteCallback, _class, displayText);
            }

            return new MvcHtmlString(anchor);
        }


        #endregion





    }

}