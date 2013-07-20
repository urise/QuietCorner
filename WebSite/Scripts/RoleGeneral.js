
$(document).ready(function () {

    $("body").on('click', "#clearButton", function (e) {
        e.preventDefault();
        resetFilters();
        $('#applyFilters').click();
    });

    $("body").on('paste keyup', '#roleName', function () { if ($(this).val().length > 128) this.value = this.value.slice(0, 128); });

    window.setupNewRow = function (button) {
        window.ajaxCallSingleton(window.NewRolePath, 'Get', null, function (result) {
            roleDialog(result, "Новая роль");
        }, null);
    };

    window.setupEditRow = function (row) {
        var roleId = row.attr('id');
        window.ajaxCallSingleton(window.EditRolePath, 'Post', { id: roleId }, function (result) {
            roleDialog(result, "Редактирование роли");
        }, null);
    };

    window.deleteRow = function (row) {
        var currentRow = $(row);
        var userNames = currentRow.attr('data-users');
        if (userNames != null && userNames != "") {
            alert("Удаляемая роль предоставлена для пользователей: " + userNames + ". Отключите роль у всех пользователей перед удалением.");
            return false;
        }
        var id = currentRow.attr('id');
        confirm('Вы уверены, что хотите удалить роль?', null, deleteRole, id);
    };

    window.setupCopyRow = function (row) {
        var roleId = row.attr('id');
        window.ajaxCallSingleton(window.CopyRolePath, 'Post', { id: roleId }, function (result) {
            roleDialog(result, "Новая роль");
        }, null);
    };

});

function updateSubComponents(self) {
    var parent = self.closest('tr').data('component');
    var value = self.val();
    $('.role-row[data-parent="' + parent + '"] input').each(function () {
        var sub = $(this);
        if (sub.val() == value) {
            sub.prop('checked', true);
        } else {
            sub.removeAttr('checked');
        }
        if (value == '1')
            sub.prop('disabled', true);
        else
            sub.prop('disabled', false);
    });
}

function resetFilters() {
    $('#SearchString').val("");
}

function roleDialog(result, header) {
    var div = $('<div>');
    div.html(result);
    div.attr('title', header);
    div.dialog({
        autoOpen: true,
        modal: true,
        width: 720,
        height: 680,
        draggable: false,
        resizable: false,
        close: function (event, ui) {
            div.remove();
        },
        create: function () {
            $(this).closest(".ui-dialog.ui-widget").addClass('roleDialog');
            $('.role-row[data-parent=""] input:checked').each(function () {
                var parent = $(this).closest('tr').data('component');
                var value = $(this).val();
                if (value == '1') {
                    $('.role-row[data-parent="' + parent + '"] input').each(function () {
                        var sub = $(this);
                        sub.prop('disabled', true);
                    });
                }
            });
        },
        buttons: [
            {
                text: "OK",
                'class': 'sbmtIndividCalculation agreeBtn',
                click: function () {
                    saveRoleHandler(function () { div.remove(); });
                }
            },
            {
                text: "Сохранить",
                'class': 'bw-btn-long save-btn',
                click: saveRoleHandler
            },
            {
                text: "Отмена",
                'class': 'cancelIndividCalculation',
                click: function () {
                    $(this).dialog("close");
                    div.remove();
                    return false;
                }
            }
        ]
    });    
    $(".save-btn").focus();
}

function getRoleData() {
    var role = {
        RoleId : $("#RoleId").val(),
        Name: $("#Name").val(),
        Type: $("#Type").val(),
        UserNames: $('#UserNames').val(),
        Components : []
    };
    var rows = $('.role-row');
    rows.each(function (i) {
        var self = $(this);
        var access = self.find('input:radio:checked').val();
        var component = {
            ComponentId: self.data("component"),
            Access: parseInt(access)
        };
        role.Components.push(component);
    });

    return role;
}

function saveRoleHandler(callback) {
    if ($("#Name").val().match(/^\s*$/)) { alert("Заполните все поля."); return false; }

    window.ajaxCallSingleton(window.UpdateRolePath, 'post', getRoleData(), function (result) {
        if (result.IsPermissionsChanged) window.location.href = window.location.href;
        else {
            var newRow = $($.parseHTML(result));
            var roleId = $("#RoleId").val();
            //add or update table row from result
            if (roleId && roleId * 1 > 0) {
                //TODO: replace just name
                $("#" + roleId).replaceWith(newRow);
            }
            else
                newRow.appendTo($('#roles'));

            $("#RoleId").val(newRow.attr('id'));

            $('.emty-table-row').hide();
            if ($.isFunction(callback)) callback();          
        }
    }, null);
}

function deleteRole(roleId) {
    window.ajaxCallSingleton(
        window.DeleteRolePath, 'Post', { roleId: roleId },
        function (response) {
                $('#' + roleId).remove();
        }, null);
    return true;
}