import React, { useEffect, useState } from "react";
import {
  Card,
  Table,
  Button,
  Modal,
  Form,
  Input,
  Switch,
  message,
  Space,
  Select,
} from "antd";
import {
  fetchFieldDefinitions,
  createFieldDefinition,
  updateFieldDefinition,
  deleteFieldDefinition,
} from "../api";

const fieldTypes = [
  { label: "متن", value: "string" },
  { label: "عدد", value: "number" },
  { label: "تاریخ", value: "date" },
  { label: "بله/خیر", value: "boolean" },
];

export default function FieldDefinitions() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [editing, setEditing] = useState(null);
  const [modalVisible, setModalVisible] = useState(false);
  const [form] = Form.useForm();

  const load = async () => {
    setLoading(true);
    try {
      const data = await fetchFieldDefinitions();
      setItems(data);
    } catch {
      message.error("خطا در بارگذاری فیلدها");
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
    form.setFieldsValue({
      dataType: "string",
      isFilterable: true,
      isSortable: false,
      isActive: true,
    });
    setModalVisible(true);
  };

  const openEdit = (record) => {
    setEditing(record);
    form.setFieldsValue(record);
    setModalVisible(true);
  };

  const handleSubmit = async (values) => {
    try {
      if (editing) {
        await updateFieldDefinition(editing.id, values);
        message.success("فیلد به‌روزرسانی شد");
      } else {
        await createFieldDefinition(values);
        message.success("فیلد اضافه شد");
      }

      setModalVisible(false);
      load();
    } catch {
      message.error("خطا در ذخیره فیلد");
    }
  };

  const handleDelete = async (id) => {
    try {
      await deleteFieldDefinition(id);
      message.success("فیلد حذف شد");
      load();
    } catch {
      message.error("خطا در حذف فیلد");
    }
  };

  const columns = [
    { title: "نام سیستمی", dataIndex: "name", key: "name" },
    { title: "عنوان نمایشی", dataIndex: "displayName", key: "displayName" },
    {
      title: "نوع",
      dataIndex: "dataType",
      key: "dataType",
      render: (value) =>
        fieldTypes.find((item) => item.value === value)?.label || value,
    },
    {
      title: "قابل فیلتر",
      dataIndex: "isFilterable",
      key: "isFilterable",
      render: (value) => (value ? "بله" : "خیر"),
    },
    {
      title: "فعال",
      dataIndex: "isActive",
      key: "isActive",
      render: (value) => (value ? "بله" : "خیر"),
    },
    {
      title: "اقدامات",
      key: "actions",
      width: 160,
      render: (_, record) => (
        <Space>
          <Button size="small" onClick={() => openEdit(record)}>
            ویرایش
          </Button>
          <Button size="small" danger onClick={() => handleDelete(record.id)}>
            حذف
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <>
      <Card
        title="فیلدهای داینامیک"
        extra={<Button onClick={openCreate}>فیلد جدید</Button>}
        className="card"
      >
        <Table
          dataSource={items}
          columns={columns}
          loading={loading}
          rowKey="id"
        />
      </Card>

      <Modal
        title={editing ? "ویرایش فیلد" : "فیلد جدید"}
        open={modalVisible}
        onCancel={() => setModalVisible(false)}
        onOk={() => form.submit()}
        okText="ذخیره"
        cancelText="لغو"
      >
        <Form form={form} layout="vertical" onFinish={handleSubmit}>
          <Form.Item
            name="name"
            label="نام سیستمی"
            rules={[
              { required: true, message: "نام سیستمی الزامی است" },
              {
                pattern: /^[A-Za-z][A-Za-z0-9_]*$/,
                message:
                  "با حرف انگلیسی شروع شود و فقط شامل حروف، عدد و _ باشد",
              },
            ]}
          >
            <Input placeholder="مثلا: Density" />
          </Form.Item>
          <Form.Item name="displayName" label="عنوان نمایشی">
            <Input placeholder="مثلا: چگالی" />
          </Form.Item>
          <Form.Item name="dataType" label="نوع">
            <Select options={fieldTypes} />
          </Form.Item>
          <Form.Item
            name="isFilterable"
            label="قابل فیلتر"
            valuePropName="checked"
          >
            <Switch checkedChildren="بله" unCheckedChildren="خیر" />
          </Form.Item>
          <Form.Item
            name="isSortable"
            label="قابل مرتب‌سازی"
            valuePropName="checked"
          >
            <Switch checkedChildren="بله" unCheckedChildren="خیر" />
          </Form.Item>
          <Form.Item name="isActive" label="فعال" valuePropName="checked">
            <Switch checkedChildren="فعال" unCheckedChildren="غیرفعال" />
          </Form.Item>
        </Form>
      </Modal>
    </>
  );
}
