

/* MODAL FORMS
-----------------------------------------------*/


// encapsulates a response from an ajax dialog form call
function AjaxResult(data) {
    this.Status = data.Status;
    this.Message = data.Message;
    this.Successful = data.Status == "OK";
}

// returns true if the data object is an ajax result object
function IsAjaxResult(data) {
    return (data.Status && data.Message);
}


// opens a modal dialog with a form
function ShowModalForm() {
    var url = $(this).attr("data-url");
    var title = $(this).attr("data-title");
    var buttonSet = $(this).attr("data-buttons");
    var deleteUrl = $(this).attr("data-delete-url");
    var onDeleteCallback = $(this).attr("data-on-delete-callback");
    var onSaveCallback = $(this).attr("data-on-save-callback");
    $("#DialogContent *").remove();
    $("#DialogContent").load(url, function () {
        $(this).dialog({
            modal: true,
            title: title,
            width: 600,
            buttons: GetButtons(buttonSet),
            deleteUrl: deleteUrl,
            onDeleteCallback: onDeleteCallback,
            onSaveCallback: onSaveCallback
        }).buttonArranger();
    });
}


// arranges ajax dialog form buttons in the propper order
jQuery.fn.buttonArranger = function () {
    var pane = $(this).parents(".ui-dialog").find(".ui-dialog-buttonpane");
    var deleteButton = pane.find("button span:contains('Delete')").parent("button");
    if (deleteButton.length == 0)
        return;

    deleteButton.addClass("float-left").fadeTo(1, 0.75);
    var buttonSet = deleteButton.parents(".ui-dialog-buttonset").removeClass("ui-dialog-buttonset").css("text-align", "right");
};


// buttons coded to operate an ajax dialog form
var allButtons = {
    "Delete": AjaxDelete,
    "Save": AjaxSave,
    "Cancel": AjaxDialogCancel,
    "Close": AjaxDialogClose
}


// cancels an ajax form dialog
function AjaxDialogCancel(event, ui) {
    $(this).dialog("close");
}


// closes an ajax form dialog
function AjaxDialogClose(event, ui) {
    $(this).dialog("close");
}

// uses ajax to perform a delete action on an ajax dialog
function AjaxDelete(event, ui) {
    var formDialog = $(this);
    ConfirmationDialog("Confirm Delete", "Are you sure you want to delete this record? Press Ok to proceed.", false, AjaxDeleteCallback, null, formDialog);
}

// when a delete action has been confirmed
function AjaxDeleteCallback(formDialog) {
    var deleteCallback = formDialog.dialog("option", "onDeleteCallback");
    var url = formDialog.dialog("option", "deleteUrl");
    $.ajax(url, {
        type: "POST",
        success: function (data, textStatus, jqXHR) {
            var result = new AjaxResult(data);
            if (result.Successful) {
                // confirm it was deleted
                ConfirmationDialog("Record Deleted", "The record has been deleted", false, function () {
                    if (deleteCallback)
                        window[deleteCallback](result);

                    // close the dialog window
                    formDialog.dialog("close");
                });
            } else {
                // inform that an error occurred
                ConfirmationDialog("Delete Operation Failed", result.Message, true);
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            // a server error occurred
            ConfirmationDialog("An unexpected server event caused a failure", errorThrown, true);
        }
    });
}


// uses ajax to perform a save action on an ajax dialog
function AjaxSave(event, ui) {
    var form = $(this).find("form");
    var _self = $(this);
    var onSaveCallback = _self.dialog("option", "onSaveCallback");
    var data = form.serialize();
    var url = form.attr("action");
    var method = form.attr("method");
    $.ajax(url, {
        type: method,
        data: data,
        success: function (data, textStatus, jqXHR) {

            if (typeof data == "object") {
                if (IsAjaxResult(data)) {   // if data is an ajax result then use the ajax result data
                    var result = new AjaxResult(data);
                    if (result.Successful) {
                        try {     // if a callback was provided execute it
                            window[onSaveCallback](data);
                        } catch (e) { }

                        _self.dialog("close");
                    } else {
                        ConfirmationDialog("Save Operation Failed", result.Message, true);  // if an error occurred then show a message
                    }
                } else {
                    // if a data model is returend as JSON the SetJsonValues function will update any element text wth where the id matches property name with property value
                    // eg. <span id="Name">Old Name</span> will get an updated name if the data object has {Name: "New Name"}
                    // the Html.IDTextFor(m=>m.Name) helper can be used to render such a span element with the id set to the property name
                    SetJsonValues(data);
                    _self.dialog("close");
                }
            }

            if (typeof data == "string") {
                var newForm = data;
                form.after(newForm);
                form.remove();
                form = newForm;
            }
        },
        // jqXHR jqXHR, String textStatus, String errorThrown 
        error: function (jqXHR, textStatus, errorThrown) {
            alert(errorThrown);
        }
    });
    // do not redirect if form submits
}


// returns a string array by parsing a comma separated value string and removing spaces
function GetButtons(buttonSet) {
    var buttons = new Object();

    // the default is SaveCancel
    if (!buttonSet || buttonSet == null || buttonSet == "SaveCancel") {
        buttons["Save"] = allButtons["Save"];
        buttons["Cancel"] = allButtons["Cancel"];
    }

    // Save Cancel Delete
    if (buttonSet == "SaveCancelDelete") {
        buttons["Delete"] = allButtons["Delete"];
        buttons["Save"] = allButtons["Save"];
        buttons["Cancel"] = allButtons["Cancel"];
    }

    // Close Button
    if (buttonSet == "Close") {
        buttons["Close"] = allButtons["Close"];
    }

    return buttons
}


// loops through a Json object and updates the text of elements where the id matches the property name eg. $("#FirstName").text(Json["FirstName"]);
function SetJsonValues(json) {
    for (var id in json) {
        var value = json[id];
        if (value == null)
            $("#" + id).text("");
        else
            $("#" + id).text(value);

        if (typeof value == "object")
            SetJsonValues(value);
    }

}


// opens an auto-destroying modal dialog that confirm an action wtih callbacks for confirm and cancel events
// title - the dialog title
// message - the message displayed to the user
// confirmCallback - the function to call if they confirm with parameters (confirmationDialog, dataObject)
// cancelCallback - the function to call if they cancel with parameters (confirmationDialog, dataObject)
// dataObject - an object that is passed to the callback functions
function ConfirmationDialog(title, message, isError, confirmCallback, cancelCallback, dataObject) {

    var div;
    if (isError) {
        div = $("<div><strong class='error'>" + message + "</strong></div>");
    } else {
        div = $("<div><strong>" + message + "</strong></div>");
    }
    div.dialog({
        title: title,
        modal: true,
        close: function (event, ui) {
            $(this).dialog("destroy");
        },
        buttons: [
                {
                    text: "Ok", click: function () {
                        if (confirmCallback)
                            confirmCallback(dataObject);
                        $(this).dialog("close");
                    }
                },
                {
                    text: "Cancel", click: function () {
                        if (cancelCallback)
                            cancelCallback(dataObject);
                        $(this).dialog("close");
                    }
                }
        ]
    });

}

/* -----------------------------------------------
MODAL FORMS */
