$(document).ready(function () {

    $("body").on('click', '.new', function () { if ($(this).hasClass('noAccess')) return; if ($(".editor-field").length > 0) return; window.setupNewRow($(this)); });
    $("body").on('click', '.edit', function () { if ($(this).hasClass('noAccess')) return; if ($(".editor-field").length > 0) return; window.setupEditRow($(this).closest('tr')); });
    $("body").on('click', '.copy', function () { if ($(this).hasClass('noAccess')) return; if ($(".editor-field").length > 0) return; window.setupCopyRow($(this).closest('tr')); });
    $("body").on('click', '.close', function () { if ($(this).hasClass('noAccess')) return; window.closeEditorRow($(this).closest('tr')); });
    $("body").on('click', '.save', function () { if ($(this).hasClass('noAccess')) return; window.saveEditorRow($(this)); });
    $("body").on('click', '.add', function () { if ($(this).hasClass('noAccess')) return; window.addUpdateRow($(this).closest('tr')); });
    $("body").on('click', '.delete', function () { if ($(this).hasClass('noAccess')) return; if ($(".editor-field").length > 0) return false; window.deleteRow($(this).closest('tr')); });
    $("body").on('click', '.history', function () { if ($(this).hasClass('noAccess')) return; if ($(".editor-field").length > 0) return false; window.processHistory(this); });


    $(document).keydown(function (e) {
        // key for "escape"
        if (e.keyCode == 27) { e.preventDefault(); window.closeEditorRow(); }
        // key for "enter"
        if (e.keyCode == 13) {
            e.preventDefault();
            if ($(".alertBox").length)
                $("#agreeBtn").click();
            else
                $(".save").click();
        }
    });

    setupValidator();

});

function closeEditorRow() {
    $(".editor-field").remove();
    $('.under-edit').show().removeClass('under-edit');
    $("tr.dimmed").removeClass('dimmed');
    $(".sortable-table").sortable("enable");
}


function setupSortability(id, targetClass, whereToPostUpdates) {
    $("#"+ id).sortable({
        items: "." + targetClass,
        axis: "y",
        stop: function (event, ui) {
            var sortedIDs = $("#"+id).sortable('toArray');

            window.ajaxCallSingleton(
                whereToPostUpdates, 'Post', sortedIDs,
                function (r) {
                    
                }, null);

        }
    });
}

function deleteEntity(data) {
        window.ajaxCallSingleton(
            data.whereToPostDeletion, 'Post', { id: data.id },
            function () {
                var tr = data.idPrefix ? $('tr#' + data.idPrefix + data.id) : $('tr#' + data.id);
                var table = tr.closest('table');
                tr.remove();
                if ($("table .active-row").length == 0) $("table .pseudo-th").after('<tr class="emty-table-row"><td colspan="' + data.emptyRowCollspan + '">' + data.emptyRowMessage + '</td></tr>');
                //for thouse cases when we don't have pseudo rows
                if (data.idPrefix && table.find('tr').length < 2) $("table tr:first").after('<tr class="emty-table-row"><td colspan="' + data.emptyRowCollspan + '">' + data.emptyRowMessage + '</td></tr>');
                // in case of exchange rates we need to redefine new active state
                if (data.CurrencyClassId) {
                    if (typeof window.reviewCurrentActiveState !== "undefined") {
                       window.reviewCurrentActiveState(data.CurrencyClassId);
                    }
                }

            }, null);
        return true;
    }