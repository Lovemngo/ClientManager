(function () {
    var counter = 0;

    function wireAdd(buttonId, templateId, bodyId) {
        var button = document.getElementById(buttonId);
        if (!button) { return; }
        button.addEventListener('click', function () {
            var html = document.getElementById(templateId).innerHTML.replace(/__i__/g, 'n' + (counter++));
            document.getElementById(bodyId).insertAdjacentHTML('beforeend', html);
        });
    }

    wireAdd('add-address', 'address-template', 'address-rows');
    wireAdd('add-contact', 'contact-template', 'contact-rows');

    document.addEventListener('click', function (e) {
        var target = e.target;
        if (target.classList && target.classList.contains('remove-row')) {
            var row = target.closest('tr');
            if (row) { row.parentNode.removeChild(row); }
        }
    });
})();
