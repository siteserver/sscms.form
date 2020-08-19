var $url = '/form/forms';
var $urlActionsUp = '/form/forms/actions/up';
var $urlActionsDown = '/form/forms/actions/down';
var $urlExport = '/form/forms/actions/export';
var $urlImport = '/form/forms/actions/import';

var data = utils.init({
  siteId: utils.getQueryInt('siteId'),
  pageType: 'list',
  formInfoList: null,
  formInfo: null,
  urlUpload: null,
  files: []
});

var methods = {
  apiGet: function () {
    var $this = this;

    utils.loading(this, true);
    $api.get($url, {
      params: {
        siteId: this.siteId
      }
    }).then(function (response) {
      var res = response.data;

      $this.formInfoList = res.formInfoList;
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  apiDelete: function (formId) {
    var $this = this;

    utils.loading(this, true);
    $api.delete($url, {
      data: {
        siteId: this.siteId,
        formId: formId
      }
    }).then(function (response) {
      var res = response.data;

      utils.success('表单删除成功');
      $this.formInfoList = res.formInfoList;
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  btnViewClick: function (form) {
    utils.addTab('表单管理：' + form.title, utils.getRootUrl('form/data', {
      siteId: this.siteId,
      channelId: 0,
      contentId: 0,
      formId: form.id
    }));
  },

  btnUpClick: function (formInfo) {
    var $this = this;

    utils.loading(this, true);
    $api.post($urlActionsUp, {
      siteId: this.siteId,
      formId: formInfo.id
    }).then(function (response) {
      var res = response.data;

      utils.success('表单排序成功');
      $this.formInfoList = res.formInfoList;
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  btnDownClick: function (formInfo) {
    var $this = this;

    utils.loading(this, true);
    $api.post($urlActionsDown, {
      siteId: this.siteId,
      formId: formInfo.id
    }).then(function (response) {
      var res = response.data;

      utils.success('表单排序成功');
      $this.formInfoList = res.formInfoList;
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  btnEditClick: function (formInfo) {
    utils.openLayer({
      title: '修改表单',
      url: utils.getPageUrl('form', 'formsLayerAdd', {
        siteId: this.siteId,
        formId: formInfo.id
      }),
      width: 500,
      height: 300
    })
  },

  btnAddClick: function () {
    utils.openLayer({
      title: '新增表单',
      url: utils.getPageUrl('form', 'formsLayerAdd', {
        siteId: this.siteId
      }),
      width: 500,
      height: 300
    })
  },

  btnDeleteClick: function (formInfo) {
    var $this = this;

    utils.alertDelete({
      title: '删除表单',
      text: '此操作将删除表单' + formInfo.title + '，确定吗？',
      callback: function () {
        $this.apiDelete(formInfo.id);
      }
    });
  },

  btnExportClick: function (formInfo) {
    var $this = this;

    utils.loading(this, true);
    $api.post($urlExport, {
      siteId: this.siteId,
      formId: formInfo.id
    }).then(function (response) {
      var res = response.data;

      window.open(res.value);
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  uploadBefore(file) {
    var re = /(\.zip)$/i;
    if(!re.exec(file.name))
    {
      utils.error('上传格式错误，请上传zip压缩包!');
      return false;
    }

    return true;
  },

  uploadProgress: function() {
    utils.loading(this, true);
  },

  uploadSuccess: function(res, file) {
    utils.loading(this, false);

    utils.success('表单导入成功');
    location.reload(true);
  },

  uploadError: function(err) {
    utils.loading(this, false);
    var error = JSON.parse(err.message);
    utils.error(error.message);
  },
};

var $vue = new Vue({
  el: '#main',
  data: data,
  methods: methods,
  created: function () {
    this.apiGet();
    this.urlUpload = $apiUrl + $urlImport + '?siteId=' + this.siteId;
  }
});