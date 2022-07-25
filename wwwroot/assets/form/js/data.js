﻿var $url = '/form/data';
var $urlExport = $url + '/actions/export';
var $urlColumns = $url + '/actions/columns';
var $urlDelete = $url + '/actions/delete';

var data = utils.init({
  siteId: utils.getQueryInt('siteId'),
  formId: utils.getQueryInt('formId'),
  navType: 'data',
  styles: null,
  allAttributeNames: [],
  listAttributeNames: [],
  isReply: false,
  total: null,
  pageSize: null,
  page: 1,
  items: [],
  columns: null,
  uploadPanel: false,
  uploadLoading: false,
  uploadList: [],
  multipleSelection: [],
});

var methods = {
  apiGet: function (page) {
    var $this = this;

    utils.loading(this, true);
    $api.get($url, {
      params: {
        siteId: this.siteId,
        formId: this.formId,
        page: page
      }
    }).then(function (response) {
      var res = response.data;

      $this.styles = res.styles;
      $this.allAttributeNames = res.allAttributeNames;
      $this.listAttributeNames = res.listAttributeNames;
      $this.isReply = res.isReply;

      $this.items = res.items;
      $this.total = res.total;
      $this.pageSize = res.pageSize;
      $this.columns = res.columns;
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  apiDelete: function (dataIds) {
    var $this = this;

    utils.loading(true);
    $api.post($urlDelete, {
      siteId: this.siteId,
      formId: this.formId,
      dataIds: dataIds
    }).then(function (response) {
      var res = response.data;

      $this.items = res.items;
      $this.total = res.total;
      $this.pageSize = res.pageSize;
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  apiColumns: function(attributeNames) {
    var $this = this;

    $api.post($urlColumns, {
      siteId: this.siteId,
      formId: this.formId,
      attributeNames: attributeNames
    }).then(function(response) {
      var res = response.data;

      $this.listAttributeNames = attributeNames;
    }).catch(function(error) {
      utils.error(error);
    });
  },

  handleCurrentChange: function(val) {
    this.apiGet(val);
  },

  handleSelectionChange: function(val) {
    this.multipleSelection = val;
  },

  handleColumnsChange: function() {
    var listColumns = _.filter(this.columns, function(o) { return o.isList; });
    var attributeNames = _.map(listColumns, function(column) {
      return column.attributeName;
    });
    this.apiColumns(attributeNames);
  },

  btnEditClick: function (dataId) {
    location.href = utils.getRootUrl('form/dataAdd', {
      siteId: this.siteId,
      formId: this.formId,
      dataId: dataId
    });
  },

  btnReplyClick: function (dataId) {
    utils.openLayer({
      title: '回复',
      url: utils.getRootUrl('form/dataLayerReply', {
        siteId: this.siteId,
        formId: this.formId,
        dataId: dataId
      })
    });
  },

  btnDeleteClick: function (dataId) {
    var $this = this;

    utils.alertDelete({
      title: '删除数据',
      text: '此操作将删除数据，确定吗？',
      callback: function () {
        $this.apiDelete([dataId]);
      }
    });
  },

  btnDeleteSelectedClick: function () {
    var $this = this;

    utils.alertDelete({
      title: '删除所选数据',
      text: '此操作将删除所选数据，确定吗？',
      callback: function () {
        $this.apiDelete($this.dataIds);
      }
    });
  },

  btnImportClick: function() {
    this.uploadPanel = true;
  },

  uploadBefore(file) {
    var isExcel = file.name.indexOf('.xlsx', file.name.length - '.xlsx'.length) !== -1;
    if (!isExcel) {
      utils.error('表单数据导入文件只能是 Excel 格式!');
    }
    return isExcel;
  },

  uploadProgress: function() {
    utils.loading(this, true);
  },

  uploadSuccess: function(res, file) {
    this.uploadPanel = false;
    utils.success('成功导入表单数据！');
    this.apiGet();
  },

  uploadError: function(err) {
    utils.loading(this, false);
    var error = JSON.parse(err.message);
    utils.error(error.message);
  },

  btnExportClick: function () {
    var $this = this;
    utils.loading(true);

    $api.post($urlExport, {
      siteId: this.siteId,
      formId: this.formId
    }).then(function (response) {
      var res = response.data;

      utils.success('数据导出成功！');
      window.open(res.value);

    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  getAttributeText: function (attributeName) {
    var column = this.columns.find(function (x) {
      return x.attributeName === attributeName;
    })
    return column.displayName;
  },

  getAttributeType: function(attributeName) {
    var style = _.find(this.styles, function(o) {return o.attributeName === attributeName});
    if (style && style.inputType) return style.inputType;
    return 'Text';
  },

  getAttributeValue: function (item, attributeName) {
    return item[utils.toCamelCase(attributeName)];
  },

  largeImage: function(item, attributeName) {
    var imageUrl = this.getAttributeValue(item, attributeName);
    swal.fire({
      imageUrl: imageUrl,
      showConfirmButton: false,
    })
  },

  btnAddClick: function() {
    this.navType = 'dataAdd';
    this.btnNavClick();
  },

  btnNavClick: function() {
    location.href = utils.getRootUrl('form/' + this.navType, {
      siteId: this.siteId,
      formId: this.formId
    });
  }
};

var $vue = new Vue({
  el: '#main',
  data: data,
  methods: methods,
  computed: {
    isChecked: function() {
      return this.multipleSelection.length > 0;
    },

    dataIds: function() {
      var retVal = [];
      for (var i = 0; i < this.multipleSelection.length; i++) {
        var item = this.multipleSelection[i];
        retVal.push(item.id);
      }
      return retVal;
    },
  },
  created: function () {
    this.urlUpload = $apiUrl + '/form/data/actions/import?siteId=' + this.siteId + '&formId=' + this.formId;
    this.apiGet(1);
  },
});
