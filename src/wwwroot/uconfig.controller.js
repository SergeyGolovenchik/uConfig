(function () {
    angular.module("umbraco").controller("uConfig.DashboardController", function (localizationService, uConfigApi) {
        var vm = this;
        vm.configurationLoaded = false;

        vm.texts = {
            editorImpossibleAlertMessageTemplate: "",
            editorReadonlyAlertMessageTemplate: ""
        };

        vm.editorOpened = false;            // Indicates if editor modal is opened

        vm.editorKey = "";                  // Contains editing key
        vm.editorKeyDisabled = false;       // Indicates if editing key is disabled
        vm.editorKeyPlaceholder = "";       // Contains key placeholder that equals to key value

        vm.editorValue = "";                // Conatins editing value
        vm.editorValueDisabled = false;     // Indicates if ediiting value is disabled
        vm.editorValueProtected = false;    // Indicates if editing value is protected
        vm.editorValuePlaceholder = "";     // Contains value placeholder that equals to original value
        vm.editorAlertText = "";            // 
        vm.editorValueChanged = false;      // Indicates if value was changed

        // Replaces tokens in message with provided replacements
        let messageReplacementFn = (message, replacements) => {
            for (let replacement in replacements) {
                let replacementHtml = valueToHtml(replacements[replacement]);
                message = message.replace(replacement, replacementHtml);
            }
            return message;
        }

        // Transforms response status to toast style
        let getToastStyle = (status) => {
            switch (status.toLowerCase()) {
                case "ok": { return "positive"; }
                case "warning": { return "warning"; }
                case "error": { return "danger"; }
                default: { return "info"; }
            }
        }

        // Creates toast notification from response object with all needed transforms and operations
        let createToastNotificationFromResponse = (response) => {
            let message = messageReplacementFn(response.message, response.messageReplacements);
            let style = getToastStyle(response.status);

            createToastNotification(message, style);
        }

        // Creates toast notification with message and style
        let createToastNotification = (text, color) => {
            let newToast = document.createElement("uui-toast-notification");
            let toastLayout = document.createElement("uui-toast-notification-layout");
            toastLayout.innerHTML = text;
            newToast.appendChild(toastLayout);
            if (color) {
                newToast.setAttribute("color", color);
            }
            document.getElementById("notifications").appendChild(newToast);
        }

        // Transforms text into selectable and well-wrapping html markup
        let valueToHtml = function (value) {
            let keyEl = document.createElement("span");
            keyEl.classList.add('key');
            if (!value) {
                let valueEl = document.createElement('i');
                valueEl.textContent = "String:Empty";
                keyEl.appendChild(valueEl);
            }
            else {
                let valueParts = value.split(':');
                if (valueParts.length > 1) {
                    for (let i = 0; i < valueParts.length - 1; i++) {
                        let partEl = document.createElement('span');
                        partEl.textContent = valueParts[i];
                        let separatorEl = document.createElement('span');
                        separatorEl.textContent = ':';
                        keyEl.appendChild(partEl);
                        keyEl.appendChild(separatorEl);
                    }
                }
                let partEl = document.createElement('span');
                partEl.textContent = valueParts[valueParts.length - 1];
                keyEl.appendChild(partEl);
            }

            return keyEl.outerHTML;
        }


        vm.init = () => {
            vm.load();
            Promise.all([
                localizationService.localize("uConfig_editorImpossibleAlertMessage"),
                localizationService.localize("uConfig_editorReadonlyAlertMessage")
            ]).then(([impossibleMessage, readonlyMessage]) => {
                vm.texts.editorImpossibleAlertMessageTemplate = impossibleMessage;
                vm.texts.editorReadonlyAlertMessageTemplate = readonlyMessage;
            });
        }

        // Gets initial data from API
        vm.load = function () {
            vm.configurationLoaded = false;
            Promise.all([
                uConfigApi.getSettings(),
                uConfigApi.getConfiguration(),  
            ]).then(([settings, configuration]) => {
                vm.settings = settings;
                vm.configuration = configuration;
                vm.configuration.forEach((kvp) => {
                    kvp.providerName = vm.settings.providers[kvp.providerIndex];
                });

                vm.configurationLoaded = true;
                document.getElementById("copyright").innerHTML = vm.settings.copyright;
            });
        }

        vm.getConfigByKey = function (key) {
            return vm.configuration.find((kvp) => kvp.key == key);
        }

        vm.isControlled = function (key) {
            let kvp = vm.getConfigByKey(key);
            return kvp && kvp.providerIndex == vm.settings.uConfigIndex;
        }

        vm.isOverridePossible = function (key) {
            let kvp = vm.getConfigByKey(key);
            return kvp && kvp.providerIndex <= vm.settings.uConfigIndex;
        }

        vm.isEditingDisabled = function (key) {
            let kvp = vm.getConfigByKey(key);
            return kvp && (kvp.isReadonly || !vm.isOverridePossible(key));
        }

        vm.getAlertText = function (kvp) {
            if (kvp) {
                if (kvp.isReadonly) {
                    return vm.texts.editorReadonlyAlertMessageTemplate.replace("{providerName}", kvp.providerName);
                }
                if (!vm.isOverridePossible(kvp.key)) {
                    return vm.texts.editorImpossibleAlertMessageTemplate.replace("{providerName}", kvp.providerName);
                }
            }

            return "";
        }

        // Closes value editor
        vm.closeEditor = function () {
            vm.editorOpened = false;
        }

        vm.editorValueChange = function () {
            vm.editorValueChanged = true;
        }

        vm.updateEditor = function ($event) {
            $event.currentTarget.setAttribute("state", "waiting");

            uConfigApi.updateValue(vm.editorKey, vm.editorValue)
                .then((updateResponse) => {
                    createToastNotificationFromResponse(updateResponse);
                    vm.load();
                    vm.closeEditor();
                }, (errorResponse) => {
                    $event.currentTarget.setAttribute("state", "");
                    createToastNotificationFromResponse(errorResponse);
                });
        }

        vm.deleteEditor = function ($event) {
            $event.currentTarget.setAttribute("state", "waiting");

            uConfigApi.deleteValue(vm.editorKey)
                .then((deleteResponse) => {
                    createToastNotificationFromResponse(deleteResponse);
                    vm.load();
                    vm.closeEditor();
                }, (errorResponse) => {
                    $event.currentTarget.setAttribute("state", "");
                    createToastNotificationFromResponse(errorResponse);
                });
        }

        // Opens value editor
        vm.edit = function (kvp) {
            vm.editorValueChanged = false;
            if (kvp) { // open value editor
                vm.editorKey = kvp.key;
                vm.editorValue = kvp.value;
                vm.editorKeyDisabled = true;
                vm.editorKeyPlaceholder = kvp.key;
                vm.editorValueDisabled = vm.isEditingDisabled(kvp.key);
                vm.editorValueProtected = kvp.isProtected;
                vm.editorValuePlaceholder = kvp.value;
                vm.editorUpdateBtnDisabled = !vm.editorValueChanged;
                vm.editorAlertText = vm.getAlertText(kvp);
                vm.editorOpened = true;
            } 
        }

        vm.init();
    });
})();
