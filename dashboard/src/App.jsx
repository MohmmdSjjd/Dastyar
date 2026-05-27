import React, { useEffect, useMemo, useState } from "react";
import { Button, Layout, Menu, Typography } from "antd";
import {
  DashboardOutlined,
  AppstoreOutlined,
  UploadOutlined,
  SearchOutlined,
  TagsOutlined,
} from "@ant-design/icons";
import AddMaterial from "./components/AddProduct";
import ImportProducts from "./components/ImportProducts";
import SearchMaterials from "./components/SearchProducts";
import MaterialsList from "./components/ProductsList";
import DashboardStats from "./components/DashboardStats";
import FieldDefinitions from "./components/FieldDefinitions";
import Categories from "./components/Categories";
import Login from "./components/Login";
import { getAuthToken, setAuthToken } from "./api";

const { Sider, Header, Content } = Layout;
const { Title, Text } = Typography;

const menuItems = [
  {
    label: "داشبورد",
    key: "dashboard",
    icon: <DashboardOutlined />,
  },
  {
    label: "مواد اولیه",
    key: "materials",
    icon: <AppstoreOutlined />,
    children: [
      { label: "لیست مواد اولیه", key: "materials/list" },
      { label: "افزودن ماده اولیه", key: "materials/add" },
      {
        label: "دسته‌بندی‌ها",
        key: "materials/categories",
        icon: <TagsOutlined />,
      },
    ],
  },
  {
    label: "عملیات",
    key: "operations",
    icon: <UploadOutlined />,
    children: [
      { label: "ایمپورت", key: "operations/import" },
      { label: "جستجو", key: "operations/search", icon: <SearchOutlined /> },
      { label: "فیلدها", key: "operations/fields" },
    ],
  },
];

export default function App() {
  const [selectedKey, setSelectedKey] = useState("dashboard");
  const [token, setToken] = useState(() => getAuthToken());

  useEffect(() => {
    const handleUnauthorized = () => setToken(null);
    window.addEventListener("dastyar:unauthorized", handleUnauthorized);
    return () =>
      window.removeEventListener("dastyar:unauthorized", handleUnauthorized);
  }, []);

  const sectionTitle = useMemo(() => {
    switch (selectedKey) {
      case "materials/list":
        return "لیست مواد اولیه";
      case "materials/add":
        return "افزودن ماده اولیه";
      case "materials/categories":
        return "دسته‌بندی‌ها";
      case "operations/import":
        return "ایمپورت مواد اولیه";
      case "operations/search":
        return "جستجوی مواد اولیه";
      case "operations/fields":
        return "فیلدهای داینامیک";
      default:
        return "داشبورد";
    }
  }, [selectedKey]);

  const renderSection = () => {
    switch (selectedKey) {
      case "materials/list":
        return <MaterialsList />;
      case "materials/add":
        return <AddMaterial />;
      case "materials/categories":
        return <Categories />;
      case "operations/import":
        return <ImportProducts />;
      case "operations/search":
        return <SearchMaterials />;
      case "operations/fields":
        return <FieldDefinitions />;
      default:
        return <DashboardStats />;
    }
  };

  const handleLogin = (nextToken) => {
    setSelectedKey("dashboard");
    setToken(nextToken);
  };

  if (!token) {
    return <Login onLogin={handleLogin} />;
  }

  return (
    <Layout className="app-shell">
      <Sider
        width={286}
        breakpoint="lg"
        collapsedWidth={0}
        className="app-sider"
      >
        <div className="sidebar-logo">
          <div className="sidebar-logo-mark">دستیار</div>
          <div>
            <Title level={4} className="sidebar-title mb-0 text-white">
              داشبورد دستیار
            </Title>
            <Text className="sidebar-subtitle">مدیریت فرمول تولید</Text>
          </div>
        </div>

        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[selectedKey]}
          defaultOpenKeys={["materials", "operations"]}
          items={menuItems}
          onClick={({ key }) => setSelectedKey(key)}
        />
      </Sider>

      <Layout>
        <Header className="app-header">
          <div className="header-inner">
            <div>
              <Title level={3} className="text-white mb-0">
                {sectionTitle}
              </Title>
              <Text className="text-emerald-50">
                مواد اولیه، دسته‌بندی‌ها و افزودنی‌ها را دقیق و سریع کنترل کنید.
              </Text>
            </div>
            <Button
              className="logout-button"
              onClick={() => {
                setAuthToken(null);
                setToken(null);
              }}
            >
              خروج
            </Button>
          </div>
        </Header>

        <Content className="content-wrap">
          <div className="content-container">{renderSection()}</div>
        </Content>
      </Layout>
    </Layout>
  );
}
