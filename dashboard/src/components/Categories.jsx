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
  createMaterialAddonAssignment,
  deleteMaterialAddonAssignment,
  fetchCategories,
  fetchMaterialAddonAssignments,
  fetchMaterials,
  updateCategory,
} from "../api";

const { Text } = Typography;

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

function formatPrice(value) {
  return typeof value === "number" ? value.toLocaleString("fa-IR") : "-";
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
                treeData={buildTree(categories)}
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
    </>
  );
}
