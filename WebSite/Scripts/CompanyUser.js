
$(document).ready(function () {
    $(document).on('click', "#clearButton", function (e) {
        e.preventDefault();
        resetFilters();
        $('#applyFilters').click();
    });

    window.setupNewRow = function (button) {
                var newLine = formDynamicCompanyUserTr();
                button.closest('tr').after(newLine);
                $("tr.companyUserRow").addClass('dimmed');
                $("#userName").focus();
    };

    window.closeEditorRow = closeEditorField;
    window.saveEditorRow = saveCompanyUser;
    
    window.deleteRow = function (row) {
        var userName = row.attr('data-login');
        confirm('Вы уверены, что хотите лишить пользователя прав на компанию?', null, deleteCompanyUser, userName);
    };


    $("body").on('paste keyup', '#userName', function () { if ($(this).val().length > 128) this.value = this.value.slice(0, 128); });
    
    window.setupEditRow =  function (row) {
            var userLogin = row.attr('data-login');
            window.ajaxCallSingleton(window.ShowUser, 'Post', { userName: userLogin }, showSuccess, null);
        };

        function showSuccess(result) {
            userDialog(result, "Редактирование пользователя");
        }

        function userDialog(result, header) {
            var div = $('<div>');
            div.html(result);
            div.attr('title', header);
            div.dialog({
                autoOpen: true,
                modal: true,
                width: 480,
                height: 500,
                draggable: false,
                resizable: false,
                close: function (event, ui) {
                    div.remove();
                },
                create: function () {
                    $(this).closest(".ui-dialog.ui-widget").addClass('companyUserDialog');
                },
                buttons: [
                    {
                        text: "Ok",
                        'class': 'agreeBtn',
                        click: function () {
                            saveHandler(function () { div.remove(); });
                        }
                    },
                    {
                        text: "Сохранить",
                        'class': 'bw-btn-long save-btn userRoles-Ok-btn',
                        click: saveHandler
                    },
                    {
                        text: "Отмена",
                        'class': '',
                        click: function () {
                            $(this).dialog("close");
                            div.remove();
                            return false;
                        }
                    }]
            });
            //initDialog();
        }
    });

    function resetFilters() {
        $('#SearchString').val("");
    }

    function formDynamicCompanyUserTr() {
        return $('<tr class="editor-field" id="0"><td class="ps-name"><input type="text" value="" id="userName" placeholder = "Введите логин или адрес электронной почты"/></td>'
                    + '<td class="ps-action"><span class="save iconed-button" title="Сохранить">Сохранить</span>'
                    + '<span class="close iconed-button" title="Закрыть">Закрыть</span></td></tr>');
    }

    function closeEditorField() {
        $(".editor-field").remove();
        $('.companyRow-under-edit').show().removeClass('companyRow-under-edit');
        $("tr.companyUserRow").removeClass('dimmed');
    }

    function deleteCompanyUser(userName) {
        window.ajaxCallSingleton(window.DeleteCompanyUser, 'Post', { userName: userName }, function () { $('tr[data-login=' + userName + ']').remove(); }, null);
        return true;
    }

    function saveCompanyUser() {
        if ($("#userName").val().match(/^\s*$/)) { alert("Заполните все поля."); return false; }

        var user = { userName: $("#userName").val() };

        window.ajaxCallSingleton(window.UpdateCompanyUser, 'Post', user, processSuccessOnCompanyUserSave, null);
    }

    function processSuccessOnCompanyUserSave(response) {
        var newName = response.UserName;
        
        if ($('.companyRow-under-edit').length > 0) {
            var rowUnderEdit = $('.companyRow-under-edit');
            rowUnderEdit.find('.ps-name').text(newName);
            closeEditorField();
        }
        else {
            var newField = $(".editor-field");
            newField.find('.ps-name').text(newName);
            newField.find('.ps-action').html('<span class="edit iconed-button" title="Редактировать">Редактировать</span><span class="delete iconed-button" title="Удалить">Удалить</span>');
            newField.removeClass('editor-field').addClass('companyUserRow');
            newField.attr("data-login", newName);
            closeEditorField();
            $('.emty-pseudo-row').hide();
        }
    }

    function saveHandler(callback) {
        if ($("#Email").val().match(/^\s*$/)) { alert("Заполните электронный адрес."); return false; }

        var regex = /^\w+([-+.\']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/ ;
        if (!$("#Email").val().match(regex)) {
             alert("Электронный адрес имеет неверный формат."); return false;
         }

         if ($('#isUsed input[type=checkbox]:checked').length == 0) {
             alert("Пользователю должна быть присвоена хотя бы одна роль."); return false;
         }
         window.ajaxCallSingleton(window.SaveUser, 'post', getData(), function () { if ($.isFunction(callback)) callback(); }, null);
    }

    function getData() {
        var roles = getUserRolesData()[0];
        var data = {
            Login: $("#Login").val(),
            UserFio: $("#UserFio").val(),
            Email: $("#Email").val(),

            UserRoles: roles
        };

        return data;
    }

    function getUserRolesData()
    {
        return [
        $('#userRoles tbody tr.userRolesRow').map(function () {
            var $row = $(this);
            return {
                RoleId: $row.attr('data-roleId'),
                //UserRoleId: $row.attr('data-userRoleId'),
                Name: $row.find('#Name').text(),
                IsUsed: $row.find('#isUsed').find('input[type=checkbox]').is(':checked')
            };
        }).get()];
    }
