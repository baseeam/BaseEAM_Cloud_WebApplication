function startScanner(barCodeType) {
    // Config object for the live stream
    const liveStreamConfig = {
        inputStream: {
            type: 'LiveStream',
            name: "Live",
            constraints: {
                width: { min: 640 },
                height: { min: 480 },
                aspectRatio: { min: 1, max: 100 },
                facingMode: "environment" // or "user" for the front camera
            }
        },
        locator: {
            patchSize: "medium",
            halfSample: true
        },
        numOfWorkers: (navigator.hardwareConcurrency ? navigator.hardwareConcurrency : 4),
        decoder: {
            readers: [barCodeTypeMapper(barCodeType)]
        },
        locate: true
    };
    // Start the live stream scanner when the modal opens
    var App = {
        init: function () {
            Quagga.init(
                liveStreamConfig,
                function (err) {
                    if (err) {
                        showBSModal({ title: 'ERROR', body: "Cannot open the camera " + err.message });
                        Quagga.stop();
                        return;
                    }
                    Quagga.start();
                }
            );
        },
    };

    App.init();
    // The fallback to the file API requires a different inputStream option.
    // The rest is the same
    var fileConfig = $.extend({},
        liveStreamConfig, {
            inputStream: {
                size: 800
            }
        }
    );


    Quagga.onProcessed(function (result) {
        var drawingCtx = Quagga.canvas.ctx.overlay,
            drawingCanvas = Quagga.canvas.dom.overlay;
        if (result) {
            if (result.boxes) {
                drawingCtx.clearRect(0, 0, parseInt(drawingCanvas.getAttribute("width")), parseInt(drawingCanvas.getAttribute("height")));
                result.boxes.filter(function (box) {
                    return box !== result.box;
                }).forEach(function (box) {
                    Quagga.ImageDebug.drawPath(box, { x: 0, y: 1 }, drawingCtx, { color: "green", lineWidth: 2 });
                });
            }

            if (result.box) {
                Quagga.ImageDebug.drawPath(result.box, { x: 0, y: 1 }, drawingCtx, { color: "#00F", lineWidth: 2 });
            }

            if (result.codeResult && result.codeResult.code) {
                Quagga.ImageDebug.drawPath(result.line, { x: 'x', y: 'y' }, drawingCtx, { color: 'red', lineWidth: 3 });
            }
        }
    });

    Quagga.onDetected(function (result) {
        var code = result.codeResult.code;
        if (code) {
            Quagga.stop();
            var url = '/Asset/CheckBarcode?barcode=' + code;
            $.get(url, function (data) {
                if (data && data.Errors) {
                    showBSModal({ title: 'NOT FOUND!', body: data.Errors });
                } else {
                    showBSModal({
                        title: "Asset Name",
                        size: "large",
                        body: data.name,
                        actions: [{
                            label: 'Update',
                            cssClass: 'btn-primary',
                            onClick: function (e) {
                                updateModifiedDateTime(data.assetId, e);
                            }
                        }, {
                            label: 'Detail',
                            cssClass: 'btn-info',
                            onClick: function (e) {
                                window.location.href = '/Asset/Edit/' + data.assetId;
                            }
                        }, {
                            label: 'Cancel',
                            cssClass: 'btn-danger',
                            onClick: function (e) {
                                $(e.target).parents('.modal').modal('hide');
                            }
                        }]
                    });
                }
            });
        }
    });

    function barCodeTypeMapper(barcodeType) {
        switch (barcodeType) {
            case 'UPCA':
                return 'upc_reader';
            case 'EAN8':
                return 'ean_8_reader';
            case 'EAN13':
                return 'ean_reader';
            case 'CODE128':
                return 'code_128_reader ';
            default:
                return 'upc_reader';
        }
    }

    function updateModifiedDateTime(assetId, e) {
        const postData = {
            assetId
        };
        addAntiForgeryToken(postData);
        $.ajax({
            cache: false,
            type: "POST",
            url: "/Asset/UpdateModifiedDateTime",
            data: postData,
            success: function (data) {
                if (data && data.Errors) {
                    showErrors(data.Errors);
                } else {
                    $(e.target).parents('.modal').modal('hide');
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                showBSModal({ title: 'ERROR', body: thrownError });
            },
            traditional: true
        });
    }

    // Call Quagga.decodeSingle() for every file selected in the
    // file input
    $("#livestream_scanner input:file").on("change", function (e) {
        if (e.target.files && e.target.files.length) {
            Quagga.decodeSingle($.extend({}, fileConfig, { src: URL.createObjectURL(e.target.files[0]) }), function (result) { alert(result.codeResult.code); });
        }
    });
}