import React, { useState } from "react";
import { importMaterials } from "../api";
import { Card, Form, Upload, Button, message } from "antd";
import { UploadOutlined, InboxOutlined } from "@ant-design/icons";

const { Dragger } = Upload;

export default function ImportProducts() {
  const [file, setFile] = useState(null);
  const [loading, setLoading] = useState(false);

  const submit = async () => {
    if (!file) {
      message.warning("یک فایل اکسل انتخاب کنید");
      return;
    }

    setLoading(true);
    try {
      const result = await importMaterials(file);
      message.success(
        `ایمپورت موفق! ${result.added} اضافه شد، ${result.updated} بروزرسانی شد، ${result.skipped} رد شد`,
      );
      setFile(null);
    } catch (error) {
      const errorMessage =
        error.response?.data?.error || error.message || "خطا در آپلود فایل";
      message.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  const uploadProps = {
    name: "file",
    multiple: false,
    accept: ".xlsx,.xls",
    beforeUpload: (file) => {
      setFile(file);
      return false;
    },
    onRemove: () => setFile(null),
    fileList: file ? [file] : [],
  };

  return (
    <Card title="ایمپورت مواد اولیه" className="card" bordered={false}>
      <Form layout="vertical" onFinish={submit}>
        <Form.Item label="فایل اکسل">
          <Dragger {...uploadProps} className="upload-dragger">
            <p className="ant-upload-drag-icon">
              <InboxOutlined />
            </p>
            <p className="ant-upload-text">
              برای آپلود کلیک کنید یا فایل را بکشید
            </p>
            <p className="ant-upload-hint">فایل XLSX یا XLS را انتخاب کنید.</p>
          </Dragger>
        </Form.Item>

        <Form.Item>
          <Button
            type="primary"
            htmlType="submit"
            block
            loading={loading}
            disabled={!file}
            icon={<UploadOutlined />}
          >
            آپلود فایل
          </Button>
        </Form.Item>
      </Form>
    </Card>
  );
}
