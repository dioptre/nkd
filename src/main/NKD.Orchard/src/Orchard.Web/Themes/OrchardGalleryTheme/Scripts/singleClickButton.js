$(function () {
    setupSingleClickButtons();
});

function setupSingleClickButtons() {
    $('input[singleClickButton=true]').each(function (index) {
        wireUpButtonToSubmitForm($(this).closest('form').attr('Id'), $(this).attr('Id'));
    });
}

function wireUpButtonToSubmitForm(formId, buttonId) {

    $("#" + buttonId).one('click', function (event) {
        submitClick(event, formId);
    });

    // This handler will re-wire the event when the form is invalid.
    $("#" + formId).submit(function (event) {

        if (!$(this).valid()) {
            event.preventDefault();
            $("#" + buttonId).one('click', function (event) { submitClick(event, formId); });
        }
    });
}

function submitClick(event, formId) {
    $("#" + formId).submit();
}