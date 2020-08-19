var $url = '/form/settings';

var data = utils.init({
  siteId: utils.getQueryInt('siteId'),
  channelId: utils.getQueryInt('channelId'),
  contentId: utils.getQueryInt('contentId'),
  formId: utils.getQueryInt('formId'),
  returnUrl: utils.getQueryString('returnUrl'),
  navType: 'settings',
  pageType: 'list',
  form: null,
  styleList: [],
  attributeNames: null,
  administratorSmsNotifyKeys: null,
  userSmsNotifyKeys: null
});

var methods = {
  apiGet: function () {
    var $this = this;

    utils.loading(this, true);
    $api.get($url, {
      params: {
        siteId: this.siteId,
        channelId: this.channelId,
        contentId: this.contentId,
        formId: this.formId
      }
    }).then(function (response) {
      var res = response.data;

      $this.form = res.form;
      if ($this.form.pageSize === 0) {
        $this.form.pageSize = 30;
      }
      $this.styleList = res.styleList;
      $this.attributeNames = res.attributeNames;
      $this.administratorSmsNotifyKeys = res.administratorSmsNotifyKeys;
      $this.userSmsNotifyKeys = res.userSmsNotifyKeys;
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  submit: function () {
    var $this = this;

    var payload = {
      siteId: this.siteId,
      channelId: this.channelId,
      contentId: this.contentId,
      formId: this.formId,
      type: this.pageType
    };
    if (this.pageType === 'isClosed') {
      payload.isClosed = this.form.isClosed;
    } else if (this.pageType === 'title') {
      payload.title = this.form.title;
    } else if (this.pageType === 'description') {
      payload.description = this.form.description;
    } else if (this.pageType === 'isReply') {
      payload.isReply = this.form.isReply;
    } else if (this.pageType === 'pageSize') {
      payload.pageSize = this.form.pageSize;
    } else if (this.pageType === 'isTimeout') {
      payload.isTimeout = this.form.isTimeout;
      payload.timeToStart = this.form.timeToStart;
      payload.timeToEnd = this.form.timeToEnd;
    } else if (this.pageType === 'isCaptcha') {
      payload.isCaptcha = this.form.isCaptcha;
    } else if (this.pageType === 'isAdministratorSmsNotify') {
      payload.isAdministratorSmsNotify = this.form.isAdministratorSmsNotify;
      payload.administratorSmsNotifyTplId = this.form.administratorSmsNotifyTplId;
      payload.administratorSmsNotifyKeys = this.administratorSmsNotifyKeys.join(',');
      payload.administratorSmsNotifyMobile = this.form.administratorSmsNotifyMobile;
    } else if (this.pageType === 'isAdministratorMailNotify') {
      payload.isAdministratorMailNotify = this.form.isAdministratorMailNotify;
      payload.administratorMailNotifyAddress = this.form.administratorMailNotifyAddress;
    } else if (this.pageType === 'isUserSmsNotify') {
      payload.isUserSmsNotify = this.form.isUserSmsNotify;
      payload.userSmsNotifyTplId = this.form.userSmsNotifyTplId;
      payload.userSmsNotifyKeys = this.userSmsNotifyKeys.join(',');
      payload.userSmsNotifyMobileName = this.form.userSmsNotifyMobileName;
    }

    utils.loading(true);
    $api.post($url, payload).then(function (response) {
      var res = response.data;

      $this.pageType = 'list';
      swal2({
        toast: true,
        type: 'success',
        title: "设置保存成功",
        showConfirmButton: false,
        timer: 2000
      });
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  btnSubmitClick: function () {
    var $this = this;
    this.pageAlert = null;

    this.$validator.validate().then(function (result) {
      if (result) {
        $this.submit();
      }
    });
  },

  getAttributeText: function (attributeName) {
    if (attributeName === 'AddDate') {
      return '添加时间';
    }
    return attributeName;
  },

  btnNavClick: function() {
    location.href = utils.getRootUrl('form/' + this.navType, {
      siteId: this.siteId,
      channelId: this.channelId,
      contentId: this.contentId,
      formId: this.formId
    });
  }
};

var $vue = new Vue({
  el: "#main",
  data: data,
  methods: methods,
  created: function () {
    this.apiGet();
  }
});
