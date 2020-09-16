var $url = '/form/settings';

var data = utils.init({
  siteId: utils.getQueryInt('siteId'),
  formId: utils.getQueryInt('formId'),
  navType: 'settings',
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
        formId: this.formId
      }
    }).then(function (response) {
      var res = response.data;

      $this.form = _.assign({formId: $this.formId}, res.form);
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

  apiSubmit: function () {
    var $this = this;

    utils.loading(this, true);
    $api.post($url, this.form).then(function (response) {
      var res = response.data;

      utils.success('设置保存成功！');
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  btnSubmitClick: function () {
    var $this = this;
    this.$refs.form.validate(function(valid) {
      if (valid) {
        $this.apiSubmit();
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
