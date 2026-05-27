import React, { useState } from "react";
import { Button, Card, Form, Input, Segmented, Typography, message } from "antd";
import { LockOutlined, UserOutlined, IdcardOutlined } from "@ant-design/icons";
import { login, register, setAuthToken } from "../api";

const { Title, Text } = Typography;

export default function Login({ onLogin }) {
  const [mode, setMode] = useState("login");
  const [loading, setLoading] = useState(false);
  const [form] = Form.useForm();

  const handleSubmit = async (values) => {
    setLoading(true);
    try {
      const result =
        mode === "login"
          ? await login({
              userName: values.userName,
              password: values.password,
            })
          : await register({
              userName: values.userName,
              password: values.password,
              displayName: values.displayName,
            });

      setAuthToken(result.token);
      onLogin(result.token);
      message.success(mode === "login" ? "ورود موفق بود" : "ثبت‌نام انجام شد");
    } catch (error) {
      message.error(
        error.response?.data?.error ||
          (mode === "login" ? "خطا در ورود" : "خطا در ثبت‌نام"),
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-page">
      <Card className="login-card" bordered={false}>
        <div className="login-header">
          <div className="sidebar-logo-mark">دستیار</div>
          <div>
            <Title level={3} className="mb-0">
              داشبورد دستیار
            </Title>
            <Text type="secondary">ورود به مدیریت مواد و افزودنی‌ها</Text>
          </div>
        </div>

        <Segmented
          block
          className="auth-switch"
          value={mode}
          onChange={(value) => {
            setMode(value);
            form.resetFields();
          }}
          options={[
            { label: "ورود", value: "login" },
            { label: "ثبت‌نام", value: "register" },
          ]}
        />

        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
          className="auth-form"
        >
          {mode === "register" && (
            <Form.Item name="displayName" label="نام نمایشی">
              <Input prefix={<IdcardOutlined />} autoComplete="name" />
            </Form.Item>
          )}

          <Form.Item
            name="userName"
            label="نام کاربری"
            rules={[{ required: true, message: "نام کاربری را وارد کنید" }]}
          >
            <Input prefix={<UserOutlined />} autoComplete="username" />
          </Form.Item>

          <Form.Item
            name="password"
            label="رمز عبور"
            rules={[
              { required: true, message: "رمز عبور را وارد کنید" },
              ...(mode === "register"
                ? [{ min: 6, message: "رمز عبور حداقل ۶ کاراکتر باشد" }]
                : []),
            ]}
          >
            <Input.Password
              prefix={<LockOutlined />}
              autoComplete={mode === "login" ? "current-password" : "new-password"}
            />
          </Form.Item>

          <Button type="primary" htmlType="submit" block loading={loading}>
            {mode === "login" ? "ورود" : "ساخت حساب"}
          </Button>
        </Form>
      </Card>
    </div>
  );
}
