$(function () {
    $('body').on('click', '.stats', function (e) {
        e.preventDefault();
        $(this).parents('.type').toggleClass('expanded');
    });
});