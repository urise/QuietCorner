$(document).ready(function () {

    setupValidator();

    $.ajaxSetup({ cache: false });
    window.progressOverlay = new Overlay();
    $(document).ajaxStart(window.progressOverlay.start).ajaxStop(window.progressOverlay.stop);
    $('body').on('submit', 'form', function () {
            window.progressOverlay.start();
    });

    $("body").on('click', '#menu a, #subMenu a',
            function (e) {
                if ($(this).hasClass('keyRestricted')) {
                    e.preventDefault();
                    alert("Для доступа к данной функциональности выйдите и повторно войдите в систему, введя Финансовый ключ.");
                }
                if ($(this).hasClass('noAccess')
                    || ($(this).closest('ul').attr('id') !== "menu" && $(this).hasClass('active'))) {
                    e.preventDefault();
                }
            });

    $(document).on('click', "input.need-date-picker",
    function () {
        if ($(this).hasClass('hasDatepicker')) return;
        else {
            var culture = $("meta[name='accept-language']").attr("content");
            $.datepicker.setDefaults($.datepicker.regional[culture]);
            $(this).datepicker({
                changeMonth: true,
                changeYear: true,
                yearRange: '1950:2050',
                dateFormat: 'dd.mm.yy',
                onClose: function (date) { if (!isDate(date, ".")) this.value = ""; }
            });
            $(this).datepicker('show');
        }
    });
});

function Overlay() {
    if (Overlay.instance)
        return Overlay.instance;
    Overlay.instance = this;
    

    
    var element = $('<div id="AjaxOverlay" class="ui-widget-overlay" style="width: 100%; height: 100%; top: 0px; left: 0px;position: fixed; display:block;" />');
    $('body').append(element);
    element.zIndex(1001).hide();
    
    var options = {
        lines: 17, // The number of lines to draw
        length: 36, // The length of each line
        width: 8, // The line thickness
        radius: 34, // The radius of the inner circle
        corners: 1, // Corner roundness (0..1)
        rotate: 0, // The rotation offset
        direction: 1, // 1: clockwise, -1: counterclockwise
        color: '#f37528', // #rgb or #rrggbb
        speed: 1.3, // Rounds per second
        trail: 51, // Afterglow percentage
        shadow: true, // Whether to render a shadow
        hwaccel: true, // Whether to use hardware acceleration
        className: 'spinner', // The CSS class to assign to the spinner
        zIndex: 1002, // The z-index (defaults to 2000000000)
        top: 'auto', // Top position relative to parent in px
        left: 'auto' // Left position relative to parent in px
    };
    var spinner = new Spinner(options);

    this.delay = 400;
    
    this.start = function () {
        if (!Overlay.instance.timeout)
            Overlay.instance.timeout = setTimeout(function () {
                element.show();
            
                spinner.spin(element[0]);
            }, Overlay.instance.delay);
    };
    this.stop = function () {
        clearTimeout(Overlay.instance.timeout);
        Overlay.instance.timeout = null;
        spinner.stop();
        element.hide();
    };
}

window.alert = customAlert;
window.confirm = customConfirm;

function customAlert(message, callback, callbackparams) {

    var div = $('<div id="customAlertMessage">');
    div.html(message);
    div.attr('title', "Внимание");
    div.dialog({
        autoOpen: true,
        modal: true,
        width: 450,
        draggable: false,
        resizable: false,
        create: function () {
            $(this).closest(".ui-dialog.ui-widget").addClass('alertBox').attr('id', 'CustomAllertBox');
        },
        close: function () {
            $('.alertBox, #customAlertMessage').remove();
        },
        buttons: [{
            text: "OK",
            'class': 'bw-btn-long',
            'id': 'agreeBtn',
            'title': 'Ok',
            click: function () {
                $(this).dialog("close");
                $('.alertBox, #customAlertMessage').remove();
                if ($.isFunction(callback)) {
                    callback(callbackparams);
                }
            }
        }]
    });
}

function customConfirm(message, target, callback, callbackparams, idName, titleName, cancelCallback, cancelParams) {
    var isLink = (target != null) && target.attr('href').length > 0;
    var identifier = idName == null ? "customPromptMessage" : idName;
    var title = titleName == null ? "Внимание" : titleName;
    var div = $('<div id="customPromptMessageHolder">');
        div.html(message);
        div.attr('title', title);
        div.dialog({
            autoOpen: true,
            modal: true,
            width: 450,
            draggable: false,
            resizable: false,
            create: function () {
                $(".confirmBox").remove();
                $(this).closest(".ui-dialog.ui-widget").addClass('confirmBox').attr('id', identifier);
            },
            close: function () {
                $(".confirmBox, #customPromptMessageHolder").remove();
            },
            buttons: [{
                text: "OK",
                'class': 'bw-btn-long',
                'id': 'agreeBtn',
                'title': 'Ok',
                click: function () {
                    if (isLink) {
                        window.location = target.attr('href');
                    }
                    else {
                        if ($.isFunction(callback)) {
                            var attempt = callback(callbackparams);
                            if (!attempt) return;
                        }
                    }
                    $(this).dialog("close");
                    div.remove();
                }
            },
                {
                    text: "Отмена",
                    'class': 'bw-btn-long',
                    'id': 'disagreeBtn',
                    'title': 'Отмена',
                    click: function () {
                        if ($.isFunction(cancelCallback))
                            cancelCallback(cancelParams);
                        $(this).dialog("close");
                        $(".confirmBox, #customPromptMessageHolder").remove();
                        return false;
                    }
                }]
        });
        }

        var request;  // Stores the XMLHTTPRequest object
        var timeout;  // Stores timeout reference for long running requests

        function ajaxCallSingleton(url, type, data, successfunction, abortIn, alertCallback, oneError) {

            if (!request) {
                request = $.ajax({
                    url: url,
                    type: type,
                    contentType: 'application/json; charset=utf-8',
                    data: JSON.stringify(data),
                    success: function (response) {
                        if (response && response.Redirect) { window.location = window.logInPageUrl; return; }
                        if (response && response.ErrorMessage) {
                            alert(response.ErrorMessage, alertCallback);
                            if ($.isFunction(oneError)) { oneError(); }
                            return;
                        }
                        successfunction(response);
                    },
                    complete: function () {
                        timeout = request = null;
                    }
                });
                }

                if (abortIn) {
                    timeout = setTimeout(function () { if (request) request.abort(); }, parseInt(abortIn));
                }
        }

        function setupValidator() {
            // field length validator
            $("body").on('keyup', "input[data-maxlength]", function () {
                var maxLength = parseInt($(this).data('maxlength'));
                if (maxLength) {
                    if ($(this).val().length > maxLength) this.value = this.value.slice(0, maxLength);
                }
            });

            // field digits validator
            $("body").on('keyup', "input[data-digitsandseparator]", function () {
                var re = /[^0-9\.,]/;
                if (!re.test(this.value)) {
                    var numberOfCommas = this.value.split(",").length - 1;
                    var numberOfDots = this.value.split(".").length - 1;
                    if (numberOfCommas && numberOfDots || numberOfCommas > 1 || numberOfDots > 1) this.value = this.value.substring(0, this.value.length - 1);
                    return false;
                } else this.value = this.value.replace(re, '');
            });

            // field digits validator
            $("body").on('keyup', "input[data-onlydigits]", function () {
                var re = /[^0-9]/;
                if (!re.test(this.value)) return false;
                else this.value = this.value.replace(re, '');
            });
        }


        // valid for format dd/mm/yyyy
        function isDate(txtDate, separator) {
        var aoDate,        // needed for creating array and object
        ms,               // date in milliseconds
        month, day, year; // (integer) month, day and year
            // if separator is not defined then set '/'
            if (separator === undefined) {
                separator = '/';
            }
            // split input date to month, day and year
            aoDate = txtDate.split(separator);
            // array length should be exactly 3 (no more no less)
            if (aoDate.length !== 3) {
                return false;
            }
            // define month, day and year from array (expected format is m/d/yyyy)
            // subtraction will cast variables to integer implicitly
            day = aoDate[0] - 0;
            month = aoDate[1] - 1; // because months in JS start from 0
            year = aoDate[2] - 0;
            // test year range
            if (year < 1000 || year > 3000) {
                return false;
            }
            // convert input date to milliseconds
            ms = (new Date(year, month, day)).getTime();
            // initialize Date() object from milliseconds (reuse aoDate variable)
            aoDate = new Date();
            aoDate.setTime(ms);
            // compare input date and parts from Date() object
            // if difference exists then input date is not valid
            if (aoDate.getFullYear() !== year ||
        aoDate.getMonth() !== month ||
        aoDate.getDate() !== day) {
                return false;
            }
            // date is OK, return true
            return true;
        }


        function getWorkingDays(startDate, endDate) {
            var result = 0;

            var currentDate = new Date(startDate.getTime());
            while (currentDate <= endDate) {

                var weekDay = currentDate.getDay();
                if (weekDay != 0 && weekDay != 6)
                    result++;

                currentDate.setDate(currentDate.getDate() + 1);
            }
            return result;
        }

        function getDays(startDate, endDate) {
            var oneDay = 24*60*60*1000;;
            return Math.round(Math.abs((endDate.getTime() - startDate.getTime()) / (oneDay))) + 1;
        }

        function getBetweenDays(startDate, endDate) {
            return getDays(startDate, endDate) - 2; //exclude range days
        }


        Number.prototype.formatMoney = function (c, d, t) {
            var n = this,
                c = isNaN(c = Math.abs(c)) ? 2 : c,
                d = d == undefined ? "." : d,
                t = t == undefined ? "," : t,
                s = n < 0 ? "-" : "",
                i = parseInt(n = Math.abs(+n || 0).toFixed(c)) + "",
                j = (j = i.length) > 3 ? j % 3 : 0;
            return s + (j ? i.substr(0, j) + t : "") + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + t) + (c ? d + Math.abs(n - i).toFixed(c).slice(2) : "");
        };

        function getRuDateTime(a) {
            var da = a.split(".");
            return new Date(da[2], (da[1] - 1), da[0]);
        }

        function setupBrowserOldObject() {
            // jQuery 1.9 has removed the `$.browser` property, fancybox relies on
            // it, so we patch it here if it's missing.
            // This has been copied from jQuery migrate 1.1.1.
            if (!jQuery.browser) {
                var uaMatch = function (ua) {
                    ua = ua.toLowerCase();

                    var match = /(chrome)[ \/]([\w.]+)/.exec(ua) ||
                          /(webkit)[ \/]([\w.]+)/.exec(ua) ||
                          /(opera)(?:.*version|)[ \/]([\w.]+)/.exec(ua) ||
                          /(msie) ([\w.]+)/.exec(ua) ||
                          ua.indexOf("compatible") < 0 && /(mozilla)(?:.*? rv:([\w.]+)|)/.exec(ua) ||
                          [];

                    return {
                        browser: match[1] || "",
                        version: match[2] || "0"
                    };
                };

                matched = uaMatch(navigator.userAgent);
                browser = {};

                if (matched.browser) {
                    browser[matched.browser] = true;
                    browser.version = matched.version;
                }

                // Chrome is Webkit, but Webkit is also Safari.
                if (browser.chrome) {
                    browser.webkit = true;
                } else if (browser.webkit) {
                    browser.safari = true;
                }

                jQuery.browser = browser;
            }
        }

        Object.size = function (obj) {
            var size = 0, key;
            for (key in obj) {
                if (obj.hasOwnProperty(key)) size++;
            }
            return size;
        };