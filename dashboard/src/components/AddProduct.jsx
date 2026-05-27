import React, { useEffect, useMemo, useState } from "react";
import { createMaterial, fetchCategories } from "../api";
import { Card, Form, Input, Button, message, InputNumber, Select } from "antd";

export default function AddMaterial() {
  const [loading, setLoading] = useState(false);
  const [categories, setCategories] = useState([]);
  const [form] = Form.useForm();

  useEffect(() => {
    fetchCategories()
      .then(setCategories)
      .catch(() => message.error("خطا در دریافت دسته‌بندی‌ها"));
  }, []);

  const categoryOptions = useMemo(
    () =>
      categories.map((category) => ({
        label: `${category.name} (${category.code})`,
        value: category.code,
      })),
    [categories],
  );

  const submit = async (values) => {
    setLoading(true);
    try {
      await createMaterial({
        code: values.code || undefined,
        name: values.name,
        basePrice: values.basePrice || 0,
        unit: values.unit || undefined,
        categoryCode: values.categoryCode || undefined,
      });

      message.success("ماده اولیه با موفقیت افزوده شد");
      form.resetFields();
    } catch (error) {
      message.error(
        error.response?.data?.error ||
          error.message ||
          "خطا در ایجاد ماده اولیه",
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <Card title="افزودن ماده اولیه" className="card" bordered={false}>
      <Form form={form} layout="vertical" onFinish={submit}>
        <div className="material-form-grid">
          <Form.Item
            label="کد ماده اولیه"
            name="code"
            rules={[
              { min: 2, message: "کد ماده اولیه باید حداقل ۲ کاراکتر باشد" },
            ]}
          >
            <Input placeholder="مثلا: MAT001" />
          </Form.Item>

          <Form.Item
            label="نام ماده اولیه"
            name="name"
            rules={[{ required: true, message: "نام ماده اولیه را وارد کنید" }]}
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
        </div>

        <Form.Item>
          <Button type="primary" htmlType="submit" loading={loading}>
            افزودن ماده اولیه
          </Button>
        </Form.Item>
      </Form>
    </Card>
  );
}
