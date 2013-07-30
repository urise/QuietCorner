var table = $("#sheetTable")[0];

var hoverClass = "hoverRow";
var hoverClassReg = new RegExp("\\b" + hoverClass + "\\b");

table.onmouseover = table.onmouseout = function (e) {
    if (!e) e = window.event;
    var elem = e.target || e.srcElement;
    while (!elem.tagName || !elem.tagName.match(/td|th|table/i)) elem = elem.parentNode;

    if (elem.className.substring(0, 5) == "cell-") {
        $(".column-header").css('font-weight', 'normal');
        var num = elem.className.substring(5);
        $("#column-header-" + num).css('font-weight', 'bold');
    }

    //Если событие связано с элементом TD или TH из раздела TBODY
    if (elem.parentNode.tagName == 'TR' && elem.parentNode.parentNode.tagName == 'TBODY') {
        var row = elem.parentNode;//ряд содержащий ячейку таблицы в которой произошло событие
        //Если текущий ряд не "кликнутый" ряд, то в разисимости от события либо применяем стиль, назначая класс, либо убираем.
        if (e.type == "mouseover") {
            elem.className += " " + hoverClass;
        } else {
            elem.className = elem.className.replace(hoverClassReg, " ");
        }
        //elem.className = e.type == "mouseover" ? elem.className + " " + hoverClass : elem.className.replace(hoverClassReg, " ");
    }
};