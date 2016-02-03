/*
* Author Rickard Magnusson M33 2011
* Communication between a view and controller.
*/

var editor;

$(document).ready(function () {
    editor = CodeMirror.fromTextArea(document.getElementById("Content"), {
        lineNumbers: true
    });
});

function LoadFile(t) {
  
    var url = "M33.Layout/Load?fileName=";
    if (t == 'razor' || t=='js')
        url = "Load?fileName=";
  
    $('#Content').attr('disabled', 'disabled');

    $.ajax({
        url: url + $('#FileName option:selected').val(),
        context: document.body,
        success: function (data) {
            $("#Content").attr('disabled', '');
            editor.setValue('');
            editor.setValue(data.Content);
        },
        error: function () { alert("Could not load data"); }
    });
};

function Save(t) {

    var url = "M33.Layout/Save";
    if (t == 'razor' || t=='js')
        url = "Save";

    $("#FileName").attr('disabled', 'disabled');
    $('#Content').attr('disabled', 'disabled');
    $("#smb").attr('disabled', 'disabled');
    $.ajax({
        type: "post",
        dataType: "html",
        url: url,
        data: {
            FileName: $('#FileName option:selected').val(),
            Content: editor.getValue(),
            "__RequestVerificationToken": $("input[name=__RequestVerificationToken]").val()
        },
        success: function (data) {
            $("#Content").removeAttr("disabled");
            $("#smb").removeAttr("disabled");
            $("#FileName").removeAttr("disabled");
        }
    });
}


      