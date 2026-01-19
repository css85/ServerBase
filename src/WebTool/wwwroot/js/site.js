// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

var openItemSelectPopupCallback = null;
function openItemSelectPopup(index, callback) {
    openItemSelectPopupCallback = callback;
    var options = 'top=10, left=10, width=1100, height=760, status=no, menubar=no, toolbar=no, resizable=no';
    var popup = window.open("/Ext/ItemSelectPopup?index=" + index, "아이템 선택", options);
    popup.focus();
}

function onItemSelected(index, name, value) {
    if (openItemSelectPopupCallback !== null) {
        openItemSelectPopupCallback.call(this, index, name, value);
    }
}

var openUserSelectPopupCallback = null;
function openUserSelectPopup(callback) {
    openUserSelectPopupCallback = callback;
    var options = 'top=10, left=10, width=770, height=760, status=no, menubar=no, toolbar=no, resizable=no';
    var popup = window.open("/Ext/UserSelectPopup", "유저 선택", options);
    popup.focus();
}

function onUserSelected(userSeq) {
    if (openUserSelectPopupCallback !== null) {
        openUserSelectPopupCallback.call(this, `${ userSeq }`);
    }
}

var openCoupleSelectPopupCallback = null;
function openCoupleSelectPopup(callback) {
    openCoupleSelectPopupCallback = callback;
    var options = 'top=10, left=10, width=770, height=760, status=no, menubar=no, toolbar=no, resizable=no';
    var popup = window.open("/Ext/CoupleSelectPopup", "커플 선택", options);
    popup.focus();
}

function onCoupleSelected(coupleSeq) {
    if (openCoupleSelectPopupCallback !== null) {
        openCoupleSelectPopupCallback.call(this, `${ coupleSeq }`);
    }
}

var openFamSelectPopupCallback = null;
function openFamSelectPopup(callback) {
    openCoupleSelectPopupCallback = callback;
    var options = 'top=10, left=10, width=770, height=760, status=no, menubar=no, toolbar=no, resizable=no';
    var popup = window.open("/Ext/FamSelectPopup", "팸 선택", options);
    popup.focus();
}

function onFamSelected(famSeq) {
    if (openFamSelectPopupCallback !== null) {
        openFamSelectPopupCallback.call(this, `${ famSeq }`);
    }
}

var openLanguageTextInputPopupCallback = null;
function openLanguageTextInputPopup(text, callback, index = 0) {
    openLanguageTextInputPopupCallback = callback;
    var openLanguageTextInputPopupValue = $(document).find('#openLanguageTextInputPopupValue');
    if (openLanguageTextInputPopupValue === null ||
        openLanguageTextInputPopupValue === undefined ||
        openLanguageTextInputPopupValue.length === 0) {
        var tempNode = document.createElement("div");
        tempNode.innerHTML = '<input id="openLanguageTextInputPopupValue" type="hidden"/>';
        document.body.appendChild(tempNode);
        openLanguageTextInputPopupValue = $(document).find('#openLanguageTextInputPopupValue');
    }
    openLanguageTextInputPopupValue.val(text);
    var options = 'top=10, left=10, width=870, height=460, status=no, toolbar=no, resizable=no';
    var path = "/Ext/LanguageTextInputPopup?type=" + index;
    var popup = window.open(path, "언어 입력", options);
    popup.focus();
}

function onLanguageTextSelected(text) {
    if (openLanguageTextInputPopupCallback !== null) {
        openLanguageTextInputPopupCallback.call(this, text);
    }
}

var dateTimePickerOption_UtcNow = {
    locale: 'ko',
    icons: {
        time: 'far fa-clock',
        date: 'far fa-calendar-alt',
        today: 'far fa-calendar-check'
    },
    format: 'YYYY-MM-DD HH:mm',
    date: moment().utc(),
    autoclose: true,
};

let dateTimePickerOption_UtcNow_WithoutTime = {
    locale: 'ko',
    icons: {
        date: 'far fa-calendar-alt',
        tody: 'far fa-calendar-check',
    },
    format: 'YYYY-MM-DD',
    date: moment().utc(),
    autoclose: true,
    format: 'L'
};

var dateTimePickerOption_UtcAddOneDay = {
    locale: 'ko',
    icons: {
        time: 'far fa-clock',
        date: 'far fa-calendar-alt',
        today: 'far fa-calendar-check'
    },
    format: 'YYYY-MM-DD HH:mm',
    date: moment().utc().add(1, 'days'),
    autoclose: true,
};
