$apiUrl = utils.getQueryString('apiUrl') || $formConfigApiUrl;
$rootUrl = "/";
$token = localStorage.getItem(ACCESS_TOKEN_NAME);

var $api = axios.create({
  baseURL: $apiUrl,
  headers: {
    Authorization: "Bearer " + $token,
  },
});

var $url = '/form';

var data = utils.init({
  siteId: utils.getQueryInt('siteId') || $formConfigSiteId,
  formId: utils.getQueryInt('formId') || $formConfigFormId,
  pageType: '',
  styles: [],
  title: '',
  description: '',
  successMessage: '',
  successCallback: '',
  isSms: false,
  smsCountdown: 0,
  isCaptcha: false,
  captcha: '',
  captchaValue: '',
  captchaUrl: null,
  uploadUrl: null,
  files: [],
  form: null,
});

var methods = {
  apiGet: function () {
    var $this = this;

    utils.loading(this, true);
    var urlGet = $url + '/' + this.siteId + '/' + this.formId + '/actions/getForm';
    $api.post(urlGet).then(function (response) {
      var res = response.data;

      $this.title = res.title;
      $this.description = res.description;
      $this.successMessage = res.successMessage;
      $this.successCallback = res.successCallback;
      $this.isSms = res.isSms;
      $this.isCaptcha = res.isCaptcha;
      if ($this.isCaptcha) {
        $this.apiCaptchaLoad();
      }
      $this.styles = res.styles;
      $this.form = $this.getForm(res.styles, _.assign({
        captcha: '',
        smsMobile: '',
        smsCode: ''
      }, res.dataInfo));
      $this.pageType = 'form';
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  apiSendSms: function () {
    var $this = this;

    utils.loading(this, true);
    var urlSendSms = $url + '/' + this.siteId + '/' + this.formId + '/actions/sendSms';
    $api.post(urlSendSms, {
      mobile: this.form.smsMobile
    }).then(function (response) {
      var res = response.data;

      utils.notifySuccess('验证码发送成功，10分钟内有效');
      $this.smsCountdown = 60;
      var interval = setInterval(function () {
        $this.smsCountdown -= 1;
        if ($this.smsCountdown <= 0){
          clearInterval(interval);
        }
      }, 1000);
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  apiCaptchaLoad: function () {
    var $this = this;

    utils.loading(this, true);
    $api.post('/v1/captcha').then(function (response) {
      var res = response.data;

      $this.captchaValue = res.value;
      $this.captchaUrl = $apiUrl + '/v1/captcha/' + res.value;
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  apiCaptchaCheck: function () {
    var $this = this;

    $api.post('/v1/captcha/actions/check', {
      captcha: this.form.captcha,
      value: this.captchaValue
    }).then(function (res) {
      $this.apiSubmit();
    })
    .catch(function (error) {
      utils.error(error);
    });
  },

  apiSubmit: function () {
    var $this = this;

    utils.loading(true);
    $api.post($url + '/' + this.siteId + '/' + this.formId, _.assign({}, this.form)).then(function (response) {
      var res = response.data;

      $this.pageType = 'success';
      if ($this.successCallback) {
        var callback = $this.successCallback;
        if (callback.indexOf('parent.') === -1) {
          callback = 'parent.' + callback;
        }
        if (callback.indexOf('(') === -1) {
          callback += '()';
        }
        eval(callback);
      }
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  getUploadUrl: function(style) {
    return this.uploadUrl + '&fieldId=' + style.id;
  },

  imageUploaded: function(error, file) {
    if (!error) {
      var res = JSON.parse(file.serverId);
      var style = _.find(this.styles, function(o) { return o.id === res.fieldId; });
      style.value = res.value;
    }
  },

  imageRemoved: function(style) {
    style.value = [];
  },

  getForm: function(styles, value) {
    var form =  _.assign({}, value);
    for (var i = 0; i < styles.length; i++) {
      var style = styles[i];
      var name = utils.toCamelCase(style.attributeName);
      if (style.inputType === 'TextEditor') {
        setTimeout(function () {
          var editor = utils.getEditor(style.attributeName);
          editor.attributeName = style.attributeName;
          editor.ready(function () {
            this.addListener("contentChange", function () {
              $this.form[this.attributeName] = this.getContent();
            });
          });
        }, 100);
      } else if (style.inputType === 'CheckBox' || style.inputType === 'SelectMultiple') {
        if (!form[name] || !Array.isArray(form[name])) {
          form[name] = [];
        }
      }
    }
    return form;
  },

  getValue: function (attributeName) {
    for (var i = 0; i < this.styles.length; i++) {
      var style = this.styles[i];
      if (style.attributeName === attributeName) {
        return style.value;
      }
    }
    return '';
  },

  setValue: function (attributeName, value) {
    for (var i = 0; i < this.styles.length; i++) {
      var style = this.styles[i];
      if (style.attributeName === attributeName) {
        style.value = value;
      }
    }
  },

  isMobile: function (value) {
    return /^1[3-9]\d{9}$/.test(value);
  },

  btnSendSmsClick: function () {
    if (this.smsCountdown > 0) return;
    if (!this.form.smsMobile) {
      utils.error('手机号码不能为空');
      return;
    } else if (!this.isMobile(this.form.smsMobile)) {
      utils.error('请输入有效的手机号码');
      return;
    }

    this.apiSendSms();
  },

  btnImageClick: function (imageUrl) {
    top.utils.openImagesLayer([imageUrl]);
  },

  btnSubmitClick: function () {
    var $this = this;
    this.$refs.form.validate(function(valid) {
      if (valid) {
        if ($this.isCaptcha) {
          $this.apiCaptchaCheck();
        } else {
          $this.apiSubmit();
        }
      }
    });
  },

  btnLayerClick: function(options) {
    var query = {
      siteId: this.siteId,
      attributeName: options.attributeName
    };
    if (options.no) {
      query.no = options.no;
    }

    var args = {
      title: options.title,
      url: utils.getCommonUrl(options.name, query)
    };
    if (!options.full) {
      args.width = options.width ? options.width : 700;
      args.height = options.height ? options.height : 500;
    }
    utils.openLayer(args);
  },
};

var $vue = new Vue({
  el: "#main",
  data: data,
  methods: methods,
  created: function () {
    this.apiGet();
  }
});
