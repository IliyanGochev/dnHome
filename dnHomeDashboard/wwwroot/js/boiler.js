window.onload = function () {
    $.get("/api/v1/boiler/latest",
        function (latestData) {
            console.log(latestData);
            var temperatureDataSet = [];
            var powerModesDataSet = [];
            var flameDataSet = [];
            var setTemperatureDataSet = [];

            $.each(latestData, function (key, value) {
                var timestamp = moment.utc(value.timestamp).valueOf();
                temperatureDataSet.push({ t: timestamp, y: value.currentTemperature });
                powerModesDataSet.push({ t: timestamp, y: value.power });
                flameDataSet.push({ t: timestamp, y: value.flame });
                setTemperatureDataSet.push({ t: timestamp, y: value.setTemperature });
            });

            var chartCfg = {
                type: 'line',
                data: {
                    datasets: [
                        {
                            label: 'Температура',
                            backgroundColor: hexToRgba(getStyle('--info'), 10),
                            borderColor: getStyle('--info'),
                            pointHoverBackgroundColor: '#fff',
                            borderWidth: 2,
                            data: temperatureDataSet
                        },
                        {
                            label: 'Режим',
                            backgroundColor: 'transparent',
                            borderColor: getStyle('--success'),
                            pointHoverBackgroundColor: '#fff',
                            borderWidth: 2,
                            data: powerModesDataSet
                        },
                        {
                            label: 'Огън',
                            backgroundColor: 'transparent',
                            borderColor: getStyle('--success'),
                            pointHoverBackgroundColor: '#f00',
                            borderWidth: 2,
                            data: flameDataSet
                        },
                        {
                            label: 'Зададена температура',
                            backgroundColor: 'transparent',
                            borderColor: getStyle('--danger'),
                            pointHoverBackgroundColor: '#fff',
                            borderWidth: 1,
                            borderDash: [8, 5],
                            data: setTemperatureDataSet
                        }

                    ]
                },
                options: {
                    scales: {
                        xAxes: [{
                            type: 'time',
                            distribution: 'series',
                            ticks: {
                                source: 'fixedTimeData',
                                autoSkip: true
                            },
                            gridLines: {
                                drawOnChartArea: false
                            }
                        }],
                        yAxes: [{
                            scaleLabel: {
                                display: true,
                                labelString: 'Температура °C'
                            }
                        }]
                    },
                    legend: {
                        display: false
                    }
                },
                tooltips: {
                    intersect: false,
                    mode: 'index',
                    callbacks: {
                        label: function (tooltipItem, myData) {
                            var label = "Temp";
                            if (label) {
                                label += ': ';
                            }
                            label += parseFloat(tooltipItem.value).toFixed(2);
                            return label;
                        }
                    }
                }
            }
            var chart = new Chart($('#boiler-chart'), chartCfg);
            chart.update();
        });
}