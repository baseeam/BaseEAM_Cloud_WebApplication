function startScanner() {
    const codeReader = new ZXing.BrowserQRCodeReader();
    codeReader.decodeFromInputVideoDevice(undefined, 'video').then((result) => {
        var url = '/Asset/CheckBarcode?barcode=' + result;
        $.get(url, function (data) {
            if (data && data.Errors) {
                codeReader.reset();
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
                            codeReader.reset();
                            $(e.target).parents('.modal').modal('hide');
                        }
                    }]
                });
            }
        });
    }).catch((err) => {
        showBSModal({ title: 'ERROR', body: "Cannot open the camera " + err });
        })
};
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