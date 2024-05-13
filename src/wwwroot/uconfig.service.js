(function () {
    angular.module('umbraco.resources').factory('uConfigApi',
        function ($q, $http, umbRequestHelper) {
            let apiPath = "backoffice/uconfig/configuration/";

            return {
                getSettings: function () {
                    return umbRequestHelper.resourcePromise(
                        $http.get(apiPath + "settings"),
                        "Failed to retrieve providers information.");
                },

                getConfiguration: function () {
                    return umbRequestHelper.resourcePromise(
                        $http.get(apiPath + "configuration"),
                        "Failed to retrieve configuration.");
                },

                updateValue: function (key, value) {
                    return umbRequestHelper.resourcePromise(
                        $http.post(apiPath + "update", JSON.stringify({ key: key, value: value })),
                        `Failed to update key ${key}.`);
                },

                deleteValue: function (key) {
                    return umbRequestHelper.resourcePromise(
                        $http.post(apiPath + "delete", JSON.stringify({ key: key })),
                        `Failed to delete key ${key}.`);
                }
            };
        }
    );
})();
