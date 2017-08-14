/**
    * @ngdoc service
    * @name umbraco.resources.currentUserResource
    * @description Used for read/updates for the currently logged in user
    * 
    *
    **/
function currentUserResource($q, $http, umbRequestHelper) {

  //the factory object returned
  return {

    performSetInvitedUserPassword: function (newPassword) {

      if (!newPassword) {
        return angularHelper.rejectedPromise({ errorMsg: 'newPassword cannot be empty' });
      }

      return umbRequestHelper.resourcePromise(
        $http.post(
          umbRequestHelper.getApiUrl(
            "currentUserApiBaseUrl",
            "PostSetInvitedUserPassword"),
          angular.toJson(newPassword)),
        'Failed to change password');
    },

    /**
     * @ngdoc method
     * @name umbraco.resources.currentUserResource#changePassword
     * @methodOf umbraco.resources.currentUserResource
     *
     * @description
     * Changes the current users password
     * 
     * @returns {Promise} resourcePromise object containing the user array.
     *
     */
    changePassword: function (changePasswordArgs) {
      return umbRequestHelper.resourcePromise(
        $http.post(
          umbRequestHelper.getApiUrl(
            "currentUserApiBaseUrl",
            "PostChangePassword"),
          changePasswordArgs),
        'Failed to change password');
    }
    
  };
}

angular.module('umbraco.resources').factory('currentUserResource', currentUserResource);
