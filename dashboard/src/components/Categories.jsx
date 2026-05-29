import React, { useEffect, useMemo, useState } from "react";
import {
  Button,
  Card,
  Descriptions,
  Divider,
  Empty,
  Form,
  Input,
  InputNumber,
  message,
  Modal,
  Popconfirm,
  Select,
  Space,
  Table,
  Tree,
  Typography,
} from "antd";
import {
  createCategory,
  createMaterial,
  importNewCategoryMaterials,
  updateMaterialPrices,
  updateMaterialsDailyPricePercent,
  fetchMaterialUsageProducts,
  createMaterialAddonAssignment,
  deleteMaterialAddonAssignment,
  fetchCategories,
  fetchMaterialAddonAssignments,
  fetchMaterials,
  updateCategory,
} from "../api";

const { Text } = Typography;

function formatPrice(value) {
  return typeof value === "number" ? value.toLocaleString("fa-IR") : "-";
}

function getCategoryLabel(level, name) {
  if (level === 0) return `گروه اصلی · ${name}`;
  if (level === 1) return `گروه فرعی · ${name}`;
  return `نام مواد اولیه · ${name}`;
}

function buildTree(categories) {
  const nodesById = new Map();
  const roots = [];

  categories.forEach((category) => {
    nodesById.set(category.id, {
      key: category.code,
      title: category.name,
      children: [],
    });
  });

  categories.forEach((category) => {
    const node = nodesById.get(category.id);
    if (!node) return;

    if (category.parentCategoryId && nodesById.has(category.parentCategoryId)) {
      nodesById.get(category.parentCategoryId).children.push(node);
    } else {
      roots.push(node);
    }
  });

  return roots;
}

function getCategoryDepthMap(categories) {
  const byId = new Map(categories.map((category) => [category.id, category]));
  const memo = new Map();

  const getDepth = (categoryId) => {
    if (!categoryId) return 0;
    if (memo.has(categoryId)) return memo.get(categoryId);

    const category = byId.get(categoryId);
    if (!category || !category.parentCategoryId) {
      memo.set(categoryId, 0);
      return 0;
    }

    const depth = getDepth(category.parentCategoryId) + 1;
    memo.set(categoryId, depth);
    return depth;
  };

  return Object.fromEntries(categories.map((category) => [category.id, getDepth(category.id)]));
}

function getDescendantCategoryCodes(categories, rootCode) {
  if (!rootCode) return [];
  const byCode = new Map(categories.map((category) => [category.code, category]));
  const root = byCode.get(rootCode);
  if (!root) return [];

  const codes = [];
  const stack = [root];
  while (stack.length > 0) {
    const current = stack.pop();
    if (!current) continue;
    codes.push(current.code);
    categories
      .filter((category) => category.parentCategoryId === current.id)
      .forEach((child) => stack.push(child));
  }

  return codes;
}

export default function Categories() {
  const [categories, setCategories] = useState([]);
  const [materials, setMaterials] = useState([]);
  const [assignments, setAssignments] = useState([]);
  const [selectedCode, setSelectedCode] = useState(null);
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [editing, setEditing] = useState(null);
  const [addonModalVisible, setAddonModalVisible] = useState(false);
  const [selectedAddonCodes, setSelectedAddonCodes] = useState([]);
  const [addonQuantities, setAddonQuantities] = useState({});
  const [addonUnits, setAddonUnits] = useState({});
  const [materialModalVisible, setMaterialModalVisible] = useState(false);
  const [editingMaterial, setEditingMaterial] = useState(null);
  const [materialPriceModalVisible, setMaterialPriceModalVisible] = useState(false);
  const [materialPriceTarget, setMaterialPriceTarget] = useState(null);
  const [dailyPercentModalVisible, setDailyPercentModalVisible] = useState(false);
  const [materialImportModalVisible, setMaterialImportModalVisible] = useState(false);
  const [materialImportFile, setMaterialImportFile] = useState(null);
  const [materialUsageModalVisible, setMaterialUsageModalVisible] = useState(false);
  const [materialUsage, setMaterialUsage] = useState([]);
  const [materialUsageTitle, setMaterialUsageTitle] = useState('شناسنامه ماده اولیه');
  const [materialUsageLoading, setMaterialUsageLoading] = useState(false);
  const [materialForm] = Form.useForm();
  const [materialPriceForm] = Form.useForm();
  const [dailyPercentForm] = Form.useForm();
  const [materialImportForm] = Form.useForm();
  const [form] = Form.useForm();

  const selectedCategory = useMemo(
    () => categories.find((category) => category.code === selectedCode) || null,
    [categories, selectedCode],
  );

  const categoryOptions = useMemo(
    () =>
      categories
        .filter((category) => category.id !== editing?.id)
        .map((category) => ({
          label: `${category.name} (${category.code})`,
          value: category.code,
        })),
    [categories, editing],
  );

  const materialOptions = useMemo(
    () =>
      materials.map((material) => ({
        label: `${material.name || "-"} (${material.code})`,
        value: material.code,
      })),
    [materials],
  );

  const materialByCode = useMemo(() => {
    const map = new Map();
    materials.forEach((material) => map.set(material.code, material));
    return map;
  }, [materials]);

  const categoryDepthMap = useMemo(() => getCategoryDepthMap(categories), [categories]);
  const selectedCategoryCodes = useMemo(
    () => getDescendantCategoryCodes(categories, selectedCode),
    [categories, selectedCode],
  );
  const selectedMaterials = useMemo(
    () => materials.filter((material) => selectedCategoryCodes.includes(material.categoryCode)),
    [materials, selectedCategoryCodes],
  );

  const treeData = useMemo(() => {
    const decorate = (nodes, level = 0) =>
      nodes.map((node) => ({
        ...node,
        title: getCategoryLabel(level, node.title),
        children: node.children?.length ? decorate(node.children, level + 1) : [],
      }));

    return decorate(buildTree(categories));
  }, [categories]);

  const selectedAssignments = useMemo(() => {
    if (!selectedCategory) return [];
    return assignments.filter(
      (assignment) => assignment.targetCategoryCode === selectedCategory.code,
    );
  }, [assignments, selectedCategory]);

  const load = async () => {
    setLoading(true);
    try {
      const [categoriesData, materialsData, assignmentsData] =
        await Promise.all([
          fetchCategories(),
          fetchMaterials(),
          fetchMaterialAddonAssignments(),
        ]);

      setCategories(categoriesData);
      setMaterials(materialsData);
      setAssignments(assignmentsData);

      if (!selectedCode && categoriesData.length > 0) {
        setSelectedCode(categoriesData[0].code);
      }
    } catch {
      message.error("خطا در دریافت اطلاعات دسته‌بندی‌ها");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const openCreate = () => {
    setEditing(null);
    form.resetFields();
    setModalVisible(true);
  };

  const openEdit = (category) => {
    setEditing(category);
    form.setFieldsValue({
      code: category.code,
      name: category.name,
      parentCategoryCode: category.parentCategoryCode || undefined,
      unitDefault: category.unitDefault || undefined,
    });
    setModalVisible(true);
  };

  const handleSubmit = async (values) => {
    const payload = {
      code: values.code,
      name: values.name,
      parentCategoryCode: values.parentCategoryCode || null,
      unitDefault: values.unitDefault || null,
    };

    try {
      if (editing) {
        await updateCategory(editing.id, payload);
        message.success("دسته‌بندی به‌روزرسانی شد");
      } else {
        await createCategory(payload);
        message.success("دسته‌بندی اضافه شد");
      }

      setSelectedCode(payload.code);
      setModalVisible(false);
      setEditing(null);
      form.resetFields();
      load();
    } catch (error) {
      message.error(error.response?.data?.error || "خطا در ذخیره دسته‌بندی");
    }
  };

  const openAddons = () => {
    setSelectedAddonCodes([]);
    setAddonQuantities({});
    setAddonUnits({});
    setAddonModalVisible(true);
  };

  const handleAddonSelection = (codes) => {
    setSelectedAddonCodes(codes);
    setAddonQuantities((prev) => {
      const next = {};
      codes.forEach((code) => {
        next[code] = prev[code] || 1;
      });
      return next;
    });
    setAddonUnits((prev) => {
      const next = {};
      codes.forEach((code) => {
        const material = materialByCode.get(code);
        next[code] = prev[code] || material?.unitEffective || material?.unit || "";
      });
      return next;
    });
  };

  const handleCreateAddons = async () => {
    if (!selectedCategory || selectedAddonCodes.length === 0) {
      message.warning("حداقل یک ماده افزودنی انتخاب کنید");
      return;
    }

    try {
      for (const addonCode of selectedAddonCodes) {
        await createMaterialAddonAssignment({
          addonMaterialCode: addonCode,
          targetCategoryCode: selectedCategory.code,
          targetMaterialCode: null,
          quantity: addonQuantities[addonCode] || 1,
          unitOverride: addonUnits[addonCode] || null,
        });
      }

      message.success("افزودنی‌ها ثبت شدند");
      setAddonModalVisible(false);
      load();
    } catch (error) {
      message.error(error.response?.data?.error || "خطا در ثبت افزودنی‌ها");
    }
  };

  const handleRemoveAssignment = async (id) => {
    try {
      await deleteMaterialAddonAssignment(id);
      message.success("افزودنی حذف شد");
      load();
    } catch (error) {
      message.error(error.response?.data?.error || "خطا در حذف افزودنی");
    }
  };

  const openMaterialCreate = () => {
    materialForm.resetFields();
    materialForm.setFieldsValue({
      categoryCode: selectedCategory?.code || undefined,
      basePrice: 0,
      lastPurchasePrice: 0,
      dailyPurchasePrice: 0,
      isActive: true,
    });
    setEditingMaterial(null);
    setMaterialModalVisible(true);
  };

  const openMaterialEdit = (material) => {
    setEditingMaterial(material);
    materialForm.setFieldsValue({
      code: material.code,
      name: material.name,
      categoryCode: material.categoryCode || undefined,
      basePrice: material.basePrice || 0,
      lastPurchasePrice: material.lastPurchasePrice || material.basePrice || 0,
      dailyPurchasePrice: material.dailyPurchasePrice || material.lastPurchasePrice || material.basePrice || 0,
      unit: material.unit || undefined,
      isActive: material.isActive,
    });
    setMaterialModalVisible(true);
  };

  const handleMaterialSubmit = async (values) => {
    try {
      const payload = {
        code: values.code,
        name: values.name,
        categoryCode: values.categoryCode || null,
        basePrice: values.basePrice || 0,
        lastPurchasePrice: values.lastPurchasePrice || values.basePrice || 0,
        dailyPurchasePrice: values.dailyPurchasePrice || values.lastPurchasePrice || values.basePrice || 0,
        unit: values.unit || null,
        isActive: values.isActive ?? true,
      };

      if (editingMaterial) {
        await updateMaterialPrices(editingMaterial.id, {
          lastPurchasePrice: payload.lastPurchasePrice,
          dailyPurchasePrice: payload.dailyPurchasePrice,
          changeSource: 'manual',
        });
        message.success('قیمت ماده اولیه به‌روزرسانی شد');
      } else {
        await createMaterial(payload);
        message.success('ماده اولیه جدید ثبت شد');
      }

      setMaterialModalVisible(false);
      setEditingMaterial(null);
      materialForm.resetFields();
      load();
    } catch (error) {
      message.error(error.response?.data?.error || 'خطا در ذخیره ماده اولیه');
    }
  };

  const openMaterialPriceModal = (material) => {
    setMaterialPriceTarget(material);
    materialPriceForm.setFieldsValue({
      lastPurchasePrice: material.lastPurchasePrice || material.basePrice || 0,
      dailyPurchasePrice: material.dailyPurchasePrice || material.lastPurchasePrice || material.basePrice || 0,
    });
    setMaterialPriceModalVisible(true);
  };

  const handleMaterialPriceSubmit = async (values) => {
    if (!materialPriceTarget) return;
    try {
      await updateMaterialPrices(materialPriceTarget.id, {
        lastPurchasePrice: values.lastPurchasePrice,
        dailyPurchasePrice: values.dailyPurchasePrice,
        changeSource: 'manual',
      });
      message.success('قیمت‌ها بروزرسانی شد');
      setMaterialPriceModalVisible(false);
      setMaterialPriceTarget(null);
      materialPriceForm.resetFields();
      load();
    } catch (error) {
      message.error(error.response?.data?.error || 'خطا در بروزرسانی قیمت');
    }
  };

  const openDailyPercentModal = () => {
    if (!selectedCategory) {
      message.warning('یک دسته‌بندی انتخاب کنید');
      return;
    }
    dailyPercentForm.setFieldsValue({ percent: 0 });
    setDailyPercentModalVisible(true);
  };

  const handleDailyPercentSubmit = async (values) => {
    if (!selectedCategory) return;
    try {
      await updateMaterialsDailyPricePercent({
        categoryId: selectedCategory.id,
        percent: values.percent,
        includeChildren: true,
        changeSource: 'percent',
      });
      message.success('فی خرید روز با درصد اعمال شد');
      setDailyPercentModalVisible(false);
      dailyPercentForm.resetFields();
      load();
    } catch (error) {
      message.error(error.response?.data?.error || 'خطا در اعمال درصد');
    }
  };

  const openMaterialImportModal = () => {
    setMaterialImportFile(null);
    materialImportForm.resetFields();
    setMaterialImportModalVisible(true);
  };

  const handleImportMaterials = async () => {
    if (!selectedCategory) {
      message.warning('یک دسته‌بندی انتخاب کنید');
      return;
    }

    if (!materialImportFile) {
      message.warning('فایل Excel را انتخاب کنید');
      return;
    }

    try {
      await importNewCategoryMaterials(selectedCategory.id, materialImportFile);
      message.success('مواد اولیه جدید از Excel ثبت شد');
      setMaterialImportModalVisible(false);
      setMaterialImportFile(null);
      load();
    } catch (error) {
      message.error(error.response?.data?.error || 'خطا در ایمپورت Excel');
    }
  };

  const openUsageProducts = async (material) => {
    setMaterialUsageTitle(`${material.name || material.code || 'ماده اولیه'}`);
    setMaterialUsageModalVisible(true);
    setMaterialUsageLoading(true);
    try {
      const result = await fetchMaterialUsageProducts(material.id);
      setMaterialUsage(result.items || []);
    } catch (error) {
      message.error(error.response?.data?.error || 'خطا در دریافت محصولات');
    } finally {
      setMaterialUsageLoading(false);
    }
  };

  const assignmentColumns = [
    {
      title: "ماده افزودنی",
      key: "addon",
      render: (_, record) => (
        <span>
          {record.addonMaterialName || "-"}{" "}
          <Text type="secondary">({record.addonMaterialCode})</Text>
        </span>
      ),
    },
    {
      title: "مقدار",
      key: "quantity",
      width: 110,
      render: (_, record) => `${record.quantity || 1} ${record.unitOverride || ""}`,
    },
    {
      title: "قیمت واحد",
      key: "unitPrice",
      width: 130,
      render: (_, record) =>
        formatPrice(materialByCode.get(record.addonMaterialCode)?.basePrice),
    },
    {
      title: "جمع",
      key: "total",
      width: 130,
      render: (_, record) => {
        const material = materialByCode.get(record.addonMaterialCode);
        return formatPrice((material?.basePrice || 0) * (record.quantity || 1));
      },
    },
    {
      title: "اقدامات",
      key: "actions",
      width: 110,
      render: (_, record) => (
        <Popconfirm
          title="این افزودنی حذف شود؟"
          okText="حذف"
          cancelText="لغو"
          onConfirm={() => handleRemoveAssignment(record.id)}
        >
          <Button danger size="small">
            حذف
          </Button>
        </Popconfirm>
      ),
    },
  ];

  return (
    <>
      <Card
        title="دسته‌بندی مواد اولیه"
        className="card"
        loading={loading}
        extra={<Button onClick={openCreate}>دسته‌بندی جدید</Button>}
      >
        <div className="category-layout">
          <Card size="small" title="ساختار دسته‌بندی" className="category-tree-card">
            {categories.length === 0 ? (
              <Empty description="دسته‌بندی ثبت نشده است" />
            ) : (
              <Tree
                treeData={treeData}
                selectedKeys={selectedCode ? [selectedCode] : []}
                defaultExpandAll
                onSelect={(keys) => setSelectedCode(keys[0] || null)}
              />
            )}
          </Card>

          <Card
            size="small"
            title="جزئیات دسته‌بندی"
            className="category-detail-card"
            extra={
              selectedCategory ? (
                <Space wrap>
                  <Button onClick={() => openEdit(selectedCategory)}>
                    ویرایش
                  </Button>
                  <Button onClick={openMaterialCreate}>
                    ماده اولیه
                  </Button>
                  <Button onClick={openMaterialImportModal}>
                    Excel جدید
                  </Button>
                  <Button onClick={openDailyPercentModal}>
                    افزایش روز %
                  </Button>
                  <Button type="primary" onClick={openAddons}>
                    افزودنی‌ها
                  </Button>
                </Space>
              ) : null
            }
          >
            {!selectedCategory ? (
              <Empty description="یک دسته‌بندی انتخاب کنید" />
            ) : (
              <>
                <Descriptions bordered size="small" column={{ xs: 1, md: 2 }}>
                  <Descriptions.Item label="نام">
                    {selectedCategory.name}
                  </Descriptions.Item>
                  <Descriptions.Item label="کد">
                    <code>{selectedCategory.code}</code>
                  </Descriptions.Item>
                  <Descriptions.Item label="والد">
                    {selectedCategory.parentCategoryCode || "ریشه"}
                  </Descriptions.Item>
                  <Descriptions.Item label="واحد پیش‌فرض">
                    {selectedCategory.unitDefault || "-"}
                  </Descriptions.Item>
                </Descriptions>

                <Divider orientation="right">افزودنی‌های این دسته‌بندی</Divider>
                <Table
                  columns={assignmentColumns}
                  dataSource={selectedAssignments}
                  rowKey="id"
                  pagination={false}
                  size="small"
                  scroll={{ x: 720 }}
                  locale={{ emptyText: "افزودنی ثبت نشده است" }}
                />

                <Divider orientation="right">مواد اولیه شاخه انتخاب‌شده</Divider>
                <Table
                  columns={[
                    {
                      title: 'کد',
                      dataIndex: 'code',
                      key: 'code',
                      width: 120,
                      render: (value) => <code>{value}</code>,
                    },
                    {
                      title: 'نام',
                      dataIndex: 'name',
                      key: 'name',
                    },
                    {
                      title: 'دسته',
                      dataIndex: 'categoryName',
                      key: 'categoryName',
                    },
                    {
                      title: 'فی آخرین خرید',
                      dataIndex: 'lastPurchasePrice',
                      key: 'lastPurchasePrice',
                      width: 140,
                      render: (value) => formatPrice(value),
                    },
                    {
                      title: 'فی خرید روز',
                      dataIndex: 'dailyPurchasePrice',
                      key: 'dailyPurchasePrice',
                      width: 140,
                      render: (value) => formatPrice(value),
                    },
                    {
                      title: 'تعداد محصولات مصرف‌کننده',
                      dataIndex: 'usageProductCount',
                      key: 'usageProductCount',
                      width: 170,
                      render: (value) => formatPrice(value),
                    },
                    {
                      title: 'اقدامات',
                      key: 'actions',
                      width: 220,
                      render: (_, record) => (
                        <Space wrap>
                          <Button size="small" onClick={() => openMaterialPriceModal(record)}>
                            قیمت
                          </Button>
                          <Button size="small" onClick={() => openUsageProducts(record)}>
                            شناسنامه
                          </Button>
                          <Button size="small" onClick={() => openMaterialEdit(record)}>
                            ثبت
                          </Button>
                        </Space>
                      ),
                    },
                  ]}
                  dataSource={selectedMaterials}
                  rowKey="id"
                  pagination={{ pageSize: 8 }}
                  size="small"
                  scroll={{ x: 940 }}
                  locale={{ emptyText: 'ماده اولیه‌ای برای این شاخه ثبت نشده است' }}
                />
              </>
            )}
          </Card>
        </div>
      </Card>

      <Modal
        title={editing ? "ویرایش دسته‌بندی" : "دسته‌بندی جدید"}
        open={modalVisible}
        onCancel={() => {
          setModalVisible(false);
          setEditing(null);
        }}
        onOk={() => form.submit()}
        okText="ذخیره"
        cancelText="لغو"
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Form.Item
            name="code"
            label="کد دسته‌بندی"
            rules={[{ required: true, message: "کد دسته‌بندی الزامی است" }]}
          >
            <Input placeholder="مثلا: CAT01" />
          </Form.Item>
          <Form.Item
            name="name"
            label="نام دسته‌بندی"
            rules={[{ required: true, message: "نام دسته‌بندی الزامی است" }]}
          >
            <Input placeholder="مثلا: فلزات" />
          </Form.Item>
          <Form.Item name="parentCategoryCode" label="دسته‌بندی والد">
            <Select
              allowClear
              showSearch
              placeholder="بدون والد"
              optionFilterProp="label"
              options={categoryOptions}
            />
          </Form.Item>
          <Form.Item name="unitDefault" label="واحد پیش‌فرض">
            <Input placeholder="مثلا: کیلوگرم" />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title={`افزودنی‌های ${selectedCategory?.name || ""}`}
        open={addonModalVisible}
        onCancel={() => setAddonModalVisible(false)}
        onOk={handleCreateAddons}
        okText="ثبت افزودنی‌ها"
        cancelText="لغو"
        width={760}
      >
        <Form layout="vertical">
          <Form.Item label="انتخاب مواد افزودنی">
            <Select
              mode="multiple"
              showSearch
              optionFilterProp="label"
              placeholder="نام یا کد ماده اولیه را جستجو کنید"
              value={selectedAddonCodes}
              onChange={handleAddonSelection}
              options={materialOptions.filter(
                (option) =>
                  !selectedAssignments.some(
                    (assignment) => assignment.addonMaterialCode === option.value,
                  ),
              )}
            />
          </Form.Item>

          {selectedAddonCodes.length > 0 && (
            <div className="addon-unit-list">
              {selectedAddonCodes.map((code) => {
                const material = materialByCode.get(code);
                const quantity = addonQuantities[code] || 1;
                const total = (material?.basePrice || 0) * quantity;
                return (
                  <div className="addon-unit-row" key={code}>
                    <div className="addon-unit-info">
                      <strong>{material?.name || code}</strong>
                      <Text type="secondary" className="block">
                        {code} · قیمت واحد: {formatPrice(material?.basePrice)}
                      </Text>
                    </div>
                    <InputNumber
                      min={1}
                      precision={0}
                      value={quantity}
                      addonAfter={addonUnits[code] || "واحد"}
                      onChange={(value) =>
                        setAddonQuantities((prev) => ({
                          ...prev,
                          [code]: value || 1,
                        }))
                      }
                    />
                    <Input
                      placeholder="واحد"
                      value={addonUnits[code]}
                      onChange={(event) =>
                        setAddonUnits((prev) => ({
                          ...prev,
                          [code]: event.target.value,
                        }))
                      }
                    />
                    <Text strong className="addon-total-preview">
                      {formatPrice(total)}
                    </Text>
                  </div>
                );
              })}
            </div>
          )}
        </Form>
      </Modal>

      <Modal
        title={editingMaterial ? `ویرایش قیمت ${editingMaterial.name || editingMaterial.code || ''}` : 'ماده اولیه جدید'}
        open={materialModalVisible}
        onCancel={() => {
          setMaterialModalVisible(false);
          setEditingMaterial(null);
          materialForm.resetFields();
        }}
        onOk={() => materialForm.submit()}
        okText="ذخیره"
        cancelText="لغو"
        width={720}
      >
        <Form form={materialForm} layout="vertical" onFinish={handleMaterialSubmit}>
          <div className="material-form-grid">
            <Form.Item
              name="code"
              label="کد ماده اولیه"
              rules={[{ required: !editingMaterial, message: 'کد ماده اولیه الزامی است' }]}
            >
              <Input disabled={Boolean(editingMaterial)} placeholder="MAT-001" />
            </Form.Item>
            <Form.Item
              name="name"
              label="نام ماده اولیه"
              rules={[{ required: !editingMaterial, message: 'نام ماده اولیه الزامی است' }]}
            >
              <Input disabled={Boolean(editingMaterial)} placeholder="نام ماده اولیه" />
            </Form.Item>
            <Form.Item name="categoryCode" label="دسته‌بندی">
              <Select
                disabled={Boolean(editingMaterial)}
                allowClear
                showSearch
                optionFilterProp="label"
                options={categoryOptions}
              />
            </Form.Item>
            <Form.Item name="basePrice" label="فی پایه">
              <InputNumber className="w-full" min={0} step={1000} disabled={Boolean(editingMaterial)} />
            </Form.Item>
            <Form.Item name="lastPurchasePrice" label="فی آخرین خرید">
              <InputNumber className="w-full" min={0} step={1000} />
            </Form.Item>
            <Form.Item name="dailyPurchasePrice" label="فی خرید روز">
              <InputNumber className="w-full" min={0} step={1000} />
            </Form.Item>
            <Form.Item name="unit" label="واحد">
              <Input disabled={Boolean(editingMaterial)} placeholder="کیلوگرم" />
            </Form.Item>
          </div>
        </Form>
      </Modal>

      <Modal
        title={`ویرایش قیمت ${materialPriceTarget?.name || materialPriceTarget?.code || ''}`}
        open={materialPriceModalVisible}
        onCancel={() => {
          setMaterialPriceModalVisible(false);
          setMaterialPriceTarget(null);
          materialPriceForm.resetFields();
        }}
        onOk={() => materialPriceForm.submit()}
        okText="ذخیره"
        cancelText="لغو"
      >
        <Form form={materialPriceForm} layout="vertical" onFinish={handleMaterialPriceSubmit}>
          <Form.Item name="lastPurchasePrice" label="فی آخرین خرید" rules={[{ required: true, message: 'مقدار الزامی است' }]}>
            <InputNumber className="w-full" min={0} step={1000} />
          </Form.Item>
          <Form.Item name="dailyPurchasePrice" label="فی خرید روز" rules={[{ required: true, message: 'مقدار الزامی است' }]}>
            <InputNumber className="w-full" min={0} step={1000} />
          </Form.Item>
        </Form>
      </Modal>

      <Modal
        title="ثبت مواد اولیه جدید از Excel"
        open={materialImportModalVisible}
        onCancel={() => {
          setMaterialImportModalVisible(false);
          setMaterialImportFile(null);
        }}
        onOk={handleImportMaterials}
        okText="بارگذاری"
        cancelText="لغو"
      >
        <Form form={materialImportForm} layout="vertical">
          <Form.Item label="فایل Excel">
            <Input
              type="file"
              accept=".xlsx,.xls"
              onChange={(event) => setMaterialImportFile(event.target.files?.[0] || null)}
            />
          </Form.Item>
          <Text type="secondary">
            در این بخش فقط مواد اولیه جدید به شاخه انتخاب‌شده اضافه می‌شوند و رکوردهای تکراری نادیده گرفته می‌شوند.
          </Text>
        </Form>
      </Modal>

      <Modal
        title={materialUsageTitle}
        open={materialUsageModalVisible}
        onCancel={() => setMaterialUsageModalVisible(false)}
        footer={null}
        width={800}
      >
        <Table
          loading={materialUsageLoading}
          dataSource={materialUsage}
          rowKey={(record) => `${record.productId}-${record.sortOrder || 0}`}
          pagination={false}
          columns={[
            { title: 'کد محصول', dataIndex: 'productCode', key: 'productCode', width: 140 },
            { title: 'نام محصول', dataIndex: 'productName', key: 'productName' },
            { title: 'مقدار', dataIndex: 'quantity', key: 'quantity', width: 100 },
            { title: 'ضایعات %', dataIndex: 'wastePercent', key: 'wastePercent', width: 100 },
            { title: 'فی', dataIndex: 'unitPrice', key: 'unitPrice', width: 140, render: (value) => formatPrice(value) },
            { title: 'جمع', dataIndex: 'lineTotal', key: 'lineTotal', width: 140, render: (value) => formatPrice(value) },
          ]}
          locale={{ emptyText: 'مصرف‌کننده‌ای برای این ماده اولیه ثبت نشده است' }}
        />
      </Modal>

      <Modal
        title="اعمال درصد روی فی خرید روز"
        open={dailyPercentModalVisible}
        onCancel={() => {
          setDailyPercentModalVisible(false);
          dailyPercentForm.resetFields();
        }}
        onOk={() => dailyPercentForm.submit()}
        okText="اعمال"
        cancelText="لغو"
      >
        <Form form={dailyPercentForm} layout="vertical" onFinish={handleDailyPercentSubmit}>
          <Form.Item name="percent" label="درصد تغییر" rules={[{ required: true, message: 'درصد الزامی است' }]}>
            <InputNumber className="w-full" min={-100} max={1000} step={1} />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}
