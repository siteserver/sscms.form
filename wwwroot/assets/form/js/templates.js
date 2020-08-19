var $url = '/form/templates';

var data = utils.init({
  siteId: utils.getQueryInt('siteId'),
  formId: utils.getQueryInt('formId'),
  type: utils.getQueryString('type'),
  templateInfoList: null,
  name: null,
  templateHtml: null,
});

var methods = {
  getIconUrl: function (templateInfo) {
    return '/assets/form/templates/' + templateInfo.name + '/' + templateInfo.icon;
  },

  getCode: function (templateInfo) {
    return '<stl:form name="表单名称" type="' + templateInfo.name + '"></stl:form>';
  },

  apiGet: function () {
    var $this = this;

    utils.loading(this, true);
    $api.get($url, {
      params: {
        siteId: this.siteId,
        type: this.type
      }
    }).then(function (response) {
      var res = response.data;

      $this.templateInfoList = res.templateInfoList;
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  btnEditClick: function (name) {
    var url = 'templatesLayerEdit.html?siteId=' + this.siteId + '&type=' + this.type + '&name=' + name + '&apiUrl=' + encodeURIComponent(this.apiUrl);
    utils.openLayer({
      title: '模板设置',
      url: url
    });
  },

  btnHtmlClick: function (templateInfo) {
    var url = utils.getRootUrl('form/templateHtml', {
      siteId: this.siteId,
      type: this.type,
      name: templateInfo.name
    });
    utils.addTab('代码编辑', url);
  },

  btnPreviewClick: function (name) {
    var $this = this;
    utils.openLayer({
      title: '预览模板',
      url: 'templatesLayerPreview.html?siteId=' + $this.siteId + '&type=' + this.type + '&name=' + name + '&apiUrl=' + encodeURIComponent($this.apiUrl)
    });
  },

  btnDeleteClick: function (template) {
    var $this = this;
    utils.alertDelete({
      title: '删除模板',
      text: '此操作将删除模板' + template.name + '，确认吗？',
      callback: function () {
        utils.loading(true);
        $api.delete($url, {
          params: {
            siteId: $this.siteId,
            type: $this.type,
            name: template.name
          }
        }).then(function (response) {
          var res = response.data;

          $this.templateInfoList = res.templateInfoList;
        }).catch(function (error) {
          utils.error(error);
        }).then(function () {
          utils.loading($this, false);
        });
      }
    });
  },

  btnSubmitClick: function () {
    var $this = this;
    utils.loading(true);
    $api.post($url + '?siteId=' + this.siteId, {
      name: this.name,
      templateHtml: this.templateHtml
    }).then(function (response) {
      var res = response.data;

      utils.success('模板编辑成功！');
      $this.pageType = 'list';
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  btnNavClick: function(type) {
    utils.loading(true);
    var url = 'templates.html?siteId=' + this.siteId + '&apiUrl=' + encodeURIComponent(this.apiUrl) + '&formId=' + this.formId + '&type=' + type;
    location.href = url;
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