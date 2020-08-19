var $url = '/form/dataAdd';

var data = utils.init({
  apiUrl: utils.getQueryString('apiUrl'),
  siteId: utils.getQueryInt('siteId'),
  channelId: utils.getQueryInt('channelId'),
  contentId: utils.getQueryInt('contentId'),
  formId: utils.getQueryInt('formId'),
  dataId: utils.getQueryInt('dataId'),
  returnUrl: utils.getQueryString('returnUrl'),
  navType: 'data',
  pageType: '',
  styles: [],
  uploadUrl: null,
  files: [],
  form: null,
});

var methods = {
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

  apiGet: function () {
    var $this = this;

    utils.loading(this, true);
    $api.get($url, {
      params: {
        siteId: this.siteId,
        channelId: this.channelId,
        contentId: this.contentId,
        formId: this.formId,
        dataId: this.dataId
      }
    }).then(function (response) {
      var res = response.data;

      $this.styles = res.styles;
      $this.form = _.assign({}, res.value);

      setTimeout(function () {
        for (var i = 0; i < $this.styles.length; i++) {
          var style = $this.styles[i];
          if (style.inputType === 'TextEditor') {
            var editor = UE.getEditor(style.attributeName, {
              allowDivTransToP: false,
              maximumWords: 99999999
            });
            editor.attributeName = style.attributeName;
            editor.ready(function () {
              editor.addListener("contentChange", function () {
                $this.form[this.attributeName] = this.getContent();
              });
            });
          }
        }
      }, 100);
      
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  apiSubmit: function () {
    var $this = this;

    utils.loading(true);
    $api.post($url, _.assign({
      siteId: this.siteId,
      channelId: this.channelId,
      contentId: this.contentId,
      formId: this.formId,
      dataId: this.dataId
    }, this.form)).then(function (response) {
      var res = response.data;

      utils.success('数据保存成功！');
      $this.navType = 'data';
      $this.btnNavClick();
      
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
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

  btnImageClick: function (imageUrl) {
    top.utils.openImagesLayer([imageUrl]);
  },

  btnSubmitClick: function () {
    var $this = this;
    this.$refs.form.validate(function(valid) {
      if (valid) {
        $this.apiSubmit();
      }
    });
  },

  btnReturnClick: function() {
    this.navType = 'data';
    this.btnNavClick();
  },

  btnNavClick: function() {
    location.href = utils.getRootUrl('form/' + this.navType, {
      siteId: this.siteId,
      channelId: this.channelId,
      contentId: this.contentId,
      formId: this.formId
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