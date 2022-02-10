function updateDHWWidget() {
    $.get("/api/v1/dhw/last", function (data) {
        $('#dhw-current-temp').html("Бойлер " + data + "°");
    });
}

function updateMainInfo() {
    updateDHWWidget();
}

updateMainInfo();
setInterval(function () { updateMainInfo() }, updateInterval);