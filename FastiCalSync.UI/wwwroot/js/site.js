(function (document) {
  var tableContainer = document.getElementById("table-container");
  if (!tableContainer) return;

  var refreshTable = function () {
    var request = new XMLHttpRequest();
    request.open("GET", "", true);
    request.setRequestHeader("X-Requested-With", "XMLHttpRequest")

    request.onload = function () {
      if (request.status !== 200) return;

      tableContainer.innerHTML = request.responseText;
    };

    request.send();

    setTimeout(refreshTable, 10 * 1000);
  };

  setTimeout(refreshTable, 10 * 1000);
})(document);