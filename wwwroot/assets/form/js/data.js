var $url = '/form/data';
var $urlActionsExport = '/form/data/actions/export';
var $urlActionsColumns = '/form/data/actions/columns';

var data = utils.init({
  siteId: utils.getQueryInt('siteId'),
  formId: utils.getQueryInt('formId'),
  navType: 'data',
  styleList: null,
  allAttributeNames: [],
  listAttributeNames: [],
  isReply: false,
  total: null,
  pageSize: null,
  page: 1,
  items: [],
  columns: null,
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

      $this.styleList = res.styleList;
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

  apiDelete: function (dataId) {
    var $this = this;

    utils.loading(true);
    $api.delete($url, {
      data: {
        siteId: this.siteId,
        formId: this.formId,
        dataId: dataId
      }
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

    $api.post($urlActionsColumns, {
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
        $this.apiDelete(dataId);
      }
    });
  },

  btnExportClick: function () {
    var $this = this;
    utils.loading(true);

    $api.post($urlActionsExport, {
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
    var style = _.find(this.styleList, function(o) {return o.title === attributeName});
    if (style && style.fieldType) return style.fieldType;
    return 'Text';
  },

  getAttributeValue: function (item, attributeName) {
    return item[_.lowerFirst(attributeName)];
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
  created: function () {
    this.apiGet(1);
  }
});
