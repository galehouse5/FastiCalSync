(function ($) {
  var $tableContainer = $("#table-container");
  if ($tableContainer.length === 0) return;

  var refreshTable = function () {
    $.get('', function (data) {
      $tableContainer.html(data);
    });

    setTimeout(refreshTable, 9 * 1000);
  };

  setTimeout(refreshTable, 9 * 1000);
})(jQuery);