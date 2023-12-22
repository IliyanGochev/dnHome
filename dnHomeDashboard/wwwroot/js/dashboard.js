window.onload = function () {

function updateDHWWidget() {
    $.get("/api/v1/dhw/currentTemp",
        function (data) {
            $('#dhw-current-temp').html("Бойлер " + data + "°");
        });
}

function updateBoilerWidget() {
    $.get("/api/v1/boiler/currentTemp",
        function (data) {
            console.log(data)
            $('#boiler-current-temp').html("Котел " + data["currentTemperature"] + "°");

            switch (data['power']) {
                case 0:
                    powerMode = "Idle";
                    break;
                case 1:
                    powerMode = "Поддържане";
                    break;
                case 2:
                    powerMode = "1";
                    break;
                case 3:
                    powerMode = "2";
                    break;
                case 4:
                    powerMode = "3";
                    break;
                default:
            }

            var status = "--";
            switch (data["status"]) {
                case 0:
                    status = "Idle";
                    break;
                case 1:
                    status = "Продухване";
                    break;
                case 2:
                    status = "Чистене";
                    break;
                case 3:
                    status = "Изчакване...";
                    break;
                case 4:
                    status = "Зареждане";
                    break;
                case 5:
                    status = "Подгряване";
                    break;
                case 6:
                    status = "Запалване 1";
                    break;
                case 7:
                    status = "Запалване 2";
                    break;
                case 8:
                    status = "Разгаряне";
                    break;
                case 9:
                    status = "Горене";
                    break;
                case 10:
                    status = "Гасене";
                    break;

                default:
                    status = "--";
            }

            var errors = "";
            switch (data["errors"]) {
                case 1:
                    errors = "Проблем при запалване";
                    break;
                case 2:
                    errors = "Задръстване";
                    break;
                default:
                    errors = "";
            }

            var mode = "";
            switch (data['mode']) {
                case 0:
                    mode = "StandBy";
                    break;
                case 1:
                    mode = 'Auto';
                    break;
                case 2:
                    mode = 'Timer';
                    break;
            }

            var priority = "";

            switch (data["state"]) {
                case 0:
                    priority = "Отопление";
                    break;
                case 2:
                    priority = "Паралелни помпи";
                    break;
                case 1:
                    priority = "DHW";
                    break;
                case 3:
                    priority = "Летен режим";
            }

            $('#boiler-mode').html(mode + " / " + priority)
            $('#boiler-status').html("Статус: " + status);
            $('#boiler-power-mode').html("Режим " + powerMode);
            $('#boiler-flame').html("Огън " + data["flame"]);
            $('#boiler-errors').html(errors);
            //$('#boiler-dhw-sensor').html("Бойлер според горелка: " + data["dhw"] + "°");
        });
}




function updateConsumptionWidget() {
    $.get("/api/v1/boiler/consumption/24hours",
        function (data) {
            $('#boiler-24h-consumption').html("Консумация на пелети: " + data + " кг");
        });
}


function updateLuftdatenSensorWidget() {
    $.get("/api/v1/luftdaten/get",
        function (data) {
            $("#pm-temp").html("Температура: " + data["temperature"] + "°");
            $("#pm-humidity").html("Влажност: " + data["humidity"] + "%");
            $("#pm-pressure").html("Налягане: " + Math.round(data["pressure"] / 100) + "hPa");
            $("#pm-10").html("PM10: " + data["pM_10"] + "µg/m³");
            $("#pm-25").html("PM2.5: " + data["pM_2_5"] + "µg/m³");
        });
}

function updateMainInfo() {
    updateDHWWidget();
    updateBoilerWidget();
    updateConsumptionWidget();
    updateLuftdatenSensorWidget();
}

updateMainInfo();
setInterval(function () { updateMainInfo() }, updateInterval);


var cardChart1 = new Chart($('#card-chart1'),
    {
        type: 'line',
        data: {
            labels: ['January', 'February', 'March', 'April', 'May', 'June', 'July'],
            datasets: [
                {
                    label: 'My First dataset',
                    backgroundColor: getStyle('--primary'),
                    borderColor: 'rgba(255,255,255,.55)',
                    data: [65, 59, 84, 84, 51, 55, 40]
                }
            ]
        },
        options: {
            maintainAspectRatio: false,
            legend: {
                display: false
            },
            scales: {
                xAxes: [
                    {
                        gridLines: {
                            color: 'transparent',
                            zeroLineColor: 'transparent'
                        },
                        ticks: {
                            fontSize: 2,
                            fontColor: 'transparent'
                        }
                    }
                ],
                yAxes: [
                    {
                        display: false,
                        ticks: {
                            display: false,
                            min: 35,
                            max: 89
                        }
                    }
                ]
            },
            elements: {
                line: {
                    borderWidth: 1
                },
                point: {
                    radius: 4,
                    hitRadius: 10,
                    hoverRadius: 4
                }
            }
        }
    }); // eslint-disable-next-line no-unused-vars

var cardChart2 = new Chart($('#card-chart2'),
    {
        type: 'line',
        data: {
            labels: ['January', 'February', 'March', 'April', 'May', 'June', 'July'],
            datasets: [
                {
                    label: 'My First dataset',
                    backgroundColor: getStyle('--info'),
                    borderColor: 'rgba(255,255,255,.55)',
                    data: [1, 18, 9, 17, 34, 22, 11]
                }
            ]
        },
        options: {
            maintainAspectRatio: false,
            legend: {
                display: false
            },
            scales: {
                xAxes: [
                    {
                        gridLines: {
                            color: 'transparent',
                            zeroLineColor: 'transparent'
                        },
                        ticks: {
                            fontSize: 2,
                            fontColor: 'transparent'
                        }
                    }
                ],
                yAxes: [
                    {
                        display: false,
                        ticks: {
                            display: false,
                            min: -4,
                            max: 39
                        }
                    }
                ]
            },
            elements: {
                line: {
                    tension: 0.00001,
                    borderWidth: 1
                },
                point: {
                    radius: 4,
                    hitRadius: 10,
                    hoverRadius: 4
                }
            }
        }
    }); // eslint-disable-next-line no-unused-vars

var cardChart3 = new Chart($('#card-chart3'),
    {
        type: 'line',
        data: {
            labels: ['January', 'February', 'March', 'April', 'May', 'June', 'July'],
            datasets: [
                {
                    label: 'My First dataset',
                    backgroundColor: 'rgba(255,255,255,.2)',
                    borderColor: 'rgba(255,255,255,.55)',
                    data: [78, 81, 80, 45, 34, 12, 40]
                }
            ]
        },
        options: {
            maintainAspectRatio: false,
            legend: {
                display: false
            },
            scales: {
                xAxes: [
                    {
                        display: false
                    }
                ],
                yAxes: [
                    {
                        display: false
                    }
                ]
            },
            elements: {
                line: {
                    borderWidth: 2
                },
                point: {
                    radius: 0,
                    hitRadius: 10,
                    hoverRadius: 4
                }
            }
        }
    }); // eslint-disable-next-line no-unused-vars


$.get("/api/v1/boiler/consumption/by-week",
    function (data) {
        var weeklyLabels = [];
        var weeklyData = [];
        $.each(data,
            function (k, v) {
                weeklyLabels.push(v["day"]);
                weeklyData.push(v["consumption"]);
            });
        var cardChart4 = new Chart($('#card-chart4'),
            {
                type: 'bar',
                data: {
                    labels: weeklyLabels,
                    datasets: [
                        {
                            label: 'Consumption by Day',
                            backgroundColor: 'rgba(255,255,255,.2)',
                            borderColor: 'rgba(255,255,255,.55)',
                            data: weeklyData
                        }
                    ]
                },
                options: {
                    maintainAspectRatio: false,
                    legend: {
                        display: false
                    },
                    scales: {
                        xAxes: [
                            {
                                display: false,
                                barPercentage: 0.6
                            }
                        ],
                        yAxes: [
                            {
                                display: false
                            }
                        ]
                    }
                }
            });
    });



}