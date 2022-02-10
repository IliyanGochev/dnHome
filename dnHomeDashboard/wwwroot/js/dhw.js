window.onload = function() {
    $.get("/api/v1/dhw/latest", function (latestData) {
        
        var fixedTimeData = [];

        $.each(latestData, function (key, value) { 
            fixedTimeData.push( { t: moment.utc(value.timestamp).valueOf(), y: value.temperature});
        });


        var chartCfg = {
        type : 'line',
        data: {
            datasets: [{
                label : 'Temp',
                backgroundColor: hexToRgba(getStyle('--info'), 10),
                borderColor: getStyle('--info'),
                pointHoverBackgroundColor: '#fff',
                borderWidth: 2,
                data: fixedTimeData
            }]
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
                    label: function(tooltipItem, myData) {
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
    var chart = new Chart($('#dhw-chart'), chartCfg);
    chart.update();
});
}    