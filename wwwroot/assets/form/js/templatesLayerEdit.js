var $url = '/form/templatesLayerEdit';

var data = {
  siteId: utils.getQueryInt('siteId'),
  type: utils.getQueryString('type'),
  name: utils.getQueryString('name'),
  isSystem: utils.getQueryBoolean('isSystem'),
  pageLoad: false,
  pageAlert: null,
  templateInfo: null
};

var methods = {
  apiGet: function () {
    var $this = this;

    utils.loading(this, true);
    $api.get($url, {
      params: {
        name: this.name
      }
    }).then(function (response) {
      var res = response.data;
      $this.templateInfo = res.value;

      if ($this.isSystem) {
        $this.pageAlert = {
          type: 'warning',
          html: '提示：' + this.name + ' 为系统模板，编辑此模板需要克隆至指定文件夹'
        };
      }
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  getTemplateHtml: function() {
    return parent.$vue.getEditorContent();
  },

  apiClone: function() {
    var $this = this;

    utils.loading(this, true);
    $api.post('', {
      originalName: $this.name,
      name: $this.templateInfo.name,
      description: $this.templateInfo.description,
      templateHtml: $this.getTemplateHtml()
    }).then(function (response) {
      utils.success('模板克隆成功！');
      parent.location.href = $this.getTemplatesUrl();
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  apiEdit: function() {
    var $this = this;

    utils.loading(this, true);
    $api.put($url, {
      originalName: $this.name,
      name: $this.templateInfo.name,
      description: $this.templateInfo.description
    }).then(function (response) {

      utils.success('模板编辑成功！');
      parent.location.href = $this.getTemplatesUrl();
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  btnSubmitClick: function () {
    var $this = this;
    this.$validator.validate().then(function (result) {
      if (result) {
        
        if ($this.isSystem === 'true') {
          $this.apiClone();
        } else {
          $this.apiEdit();
        }
      }
    });
  },

  getTemplatesUrl: function() {
    return 'templates.html?siteId=' + this.siteId + '&apiUrl=' + encodeURIComponent(this.apiUrl) + '&formId=' + this.formId + '&type=' + this.type;
  }
};

var $vue = new Vue({
  el: '#main',
  data: data,
  methods: methods,
  created: function () {
    this.apiGet();
  }
});