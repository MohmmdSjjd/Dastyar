import React, { useEffect, useMemo, useState } from "react";
import {
  Form,
  Modal,
  Input,
  Switch,
  message,
  Divider,
  InputNumber,
  Select,
} from "antd";
import { fetchCategories, updateMaterial } from "../api";

export default function EditProductModal({
  product,
  visible,
  onClose,
  onSuccess,
}) {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [categories, setCategories] = useState([]);

  useEffect(() => {
    if (!visible) return;

    fetchCategories()
      .then(setCategories)
      .catch(() => message.error("خطا در دریافت دسته‌بندی‌ها"));
  }, [visible]);

  const categoryOptions = useMemo(
    () =>
      categories.map((category) => ({
        label: `${category.name} (${category.code})`,
        value: category.code,
      })),
    [categories],
  );

  const dynamicFields = useMemo(() => {
    if (!product?.dynamicFieldsJson) return [];

    try {
      const data = JSON.parse(product.dynamicFieldsJson);
      const standardFields = [
        "code",
        "name",
        "isactive",
        "categorycode",
        "baseprice",
        "unit",
      ];

      return Object.entries(data).filter(
        ([key]) => !standardFields.includes(key.toLowerCase()),
      );
    } catch (error) {
      console.error("Error parsing dynamicFieldsJson:", error);
      return [];
    }
  }, [product?.dynamicFieldsJson]);

  useEffect(() => {
    if (product && visible) {
      const values = {
        code: product.code,
        name: product.name,
        isActive: product.isActive,
        basePrice: product.basePrice ?? 0,
        unit: product.unit ?? "",
        categoryCode: product.categoryCode ?? undefined,
      };

      dynamicFields.forEach(([key, value]) => {
        values[`dynamic_${key}`] = value;
      });

      form.setFieldsValue(values);
    }
  }, [product, visible, form, dynamicFields]);

  const handleSubmit = async (values) => {
    setLoading(true);
    try {
      const dynamicFieldsJson = {};
      Object.entries(values).forEach(([key, value]) => {
        if (key.startsWith("dynamic_")) {
          dynamicFieldsJson[key.replace("dynamic_", "")] = value;
        }
      });

      await updateMaterial(product.id, {
        code: values.code,
        name: values.name,
        isActive: values.isActive,
        basePrice: values.basePrice || 0,
        unit: values.unit || null,
        categoryCode: values.categoryCode || null,
        dynamicFieldsJson:
          Object.keys(dynamicFieldsJson).length > 0
            ? JSON.stringify(dynamicFieldsJson)
            : null,
      });

      message.success("ماده اولیه با موفقیت به‌روزرسانی شد");
      onSuccess?.();
      onClose();
      form.resetFields();
    } catch (error) {
      message.error(
        error.response?.data?.error ||
          error.message ||
          "خطا در به‌روزرسانی ماده اولیه",
      );
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={`ویرایش ماده اولیه: ${product?.name || ""}`}
      open={visible}
      onCancel={onClose}
      onOk={() => form.submit()}
      confirmLoading={loading}
      okText="ذخیره"
      cancelText="لغو"
      width={680}
    >
      <Form
        form={form}
        layout="vertical"
        onFinish={handleSubmit}
        disabled={loading}
      >
        <h4 style={{ marginBottom: 16, fontWeight: "bold" }}>اطلاعات پایه</h4>

        <div className="material-form-grid">
          <Form.Item
            label="کد ماده اولیه"
            name="code"
            rules={[
              { required: true, message: "کد ماده اولیه الزامی است" },
              { min: 2, message: "کد ماده اولیه حداقل ۲ کاراکتر باشد" },
            ]}
          >
            <Input placeholder="مثلا: MAT001" />
          </Form.Item>

          <Form.Item
            label="نام ماده اولیه"
            name="name"
            rules={[{ required: true, message: "نام ماده اولیه الزامی است" }]}
          >
            <Input placeholder="نام ماده اولیه را وارد کنید" />
          </Form.Item>

          <Form.Item label="دسته‌بندی" name="categoryCode">
            <Select
              allowClear
              showSearch
              optionFilterProp="label"
              placeholder="دسته‌بندی را انتخاب کنید"
              options={categoryOptions}
            />
          </Form.Item>

          <Form.Item
            label="قیمت پایه"
            name="basePrice"
            rules={[{ type: "number", min: 0, message: "قیمت نامعتبر است" }]}
          >
            <InputNumber className="w-full" min={0} step={1000} />
          </Form.Item>

          <Form.Item label="واحد" name="unit">
            <Input placeholder="مثلا: کیلوگرم" />
          </Form.Item>

          <Form.Item
            label="وضعیت فعال"
            name="isActive"
            valuePropName="checked"
          >
            <Switch checkedChildren="فعال" unCheckedChildren="غیرفعال" />
          </Form.Item>
        </div>

        {dynamicFields.length > 0 && (
          <>
            <Divider />
            <h4
              style={{
                marginBottom: 16,
                fontWeight: "bold",
                color: "#1890ff",
              }}
            >
              فیلدهای داینامیک ({dynamicFields.length})
            </h4>
            {dynamicFields.map(([key]) => (
              <Form.Item key={key} label={key} name={`dynamic_${key}`}>
                <Input placeholder={`مقدار ${key} را وارد کنید`} />
              </Form.Item>
            ))}
          </>
        )}
      </Form>
    </Modal>
  );
}
