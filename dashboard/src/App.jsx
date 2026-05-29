import React, { useMemo, useState, useEffect } from "react";
import { Button, Layout, Menu, Typography } from "antd";
import {
  DashboardOutlined,
  AppstoreOutlined,
  TagsOutlined,
  FileTextOutlined,
  ClusterOutlined,
  BarChartOutlined,
} from "@ant-design/icons";
import ProductsList from "./components/ProductsList";
import MaterialsList from "./components/ProductsList";
import ProductCategories from "./components/Categories";
import MaterialCategories from "./components/Categories";
import Reports from "./components/Reports";
import DashboardStats from "./components/DashboardStats";
import Login from "./components/Login";
import { getAuthToken, setAuthToken } from "./api";

const { Sider, Header, Content } = Layout;
const { Title, Text } = Typography;

const menuItems = [
  { label: "داشبورد", key: "dashboard", icon: <DashboardOutlined /> },
  { label: "لیست محصولات", key: "products", icon: <FileTextOutlined /> },
  { label: "لیست مواد اولیه", key: "materials", icon: <AppstoreOutlined /> },
  { label: "دسته‌بندی محصولات", key: "product-categories", icon: <TagsOutlined /> },
  { label: "دسته‌بندی مواد اولیه", key: "material-categories", icon: <ClusterOutlined /> },
  { label: "گزارشات", key: "reports", icon: <BarChartOutlined /> },
];

export default function App() {
  const [selectedKey, setSelectedKey] = useState("dashboard");
  const [token, setToken] = useState(() => getAuthToken());

  useEffect(() => {
    const handleUnauthorized = () => setToken(null);
    window.addEventListener("dastyar:unauthorized", handleUnauthorized);
    return () => window.removeEventListener("dastyar:unauthorized", handleUnauthorized);
  }, []);

  const sectionTitle = useMemo(() => {
    switch (selectedKey) {
      case "products":
        return "لیست محصولات";
      case "materials":
        return "لیست مواد اولیه";
      case "product-categories":
        return "دسته‌بندی محصولات";
      case "material-categories":
        return "دسته‌بندی مواد اولیه";
      case "reports":
        return "گزارشات";
      default:
        return "داشبورد";
    }
  }, [selectedKey]);

  const renderSection = () => {
    switch (selectedKey) {
      case "products":
        return <ProductsList />;
      case "materials":
        return <MaterialsList />;
      case "product-categories":
        return <ProductCategories />;
      case "material-categories":
        return <MaterialCategories />;
      case "reports":
        return <Reports />;
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
      <Sider width={286} breakpoint="lg" collapsedWidth={0} className="app-sider">
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
                مدیریت محصولات، مواد اولیه، دسته‌بندی‌ها و گزارشات.
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
