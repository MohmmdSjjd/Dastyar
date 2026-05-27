import React from "react";
import { createRoot } from "react-dom/client";
import { ConfigProvider } from "antd";
import faIR from "antd/locale/fa_IR";
import App from "./App";
import "antd/dist/reset.css";
import "./styles.css";

createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <ConfigProvider
      direction="rtl"
      locale={faIR}
      theme={{
        token: {
          colorPrimary: "#059669",
          borderRadius: 8,
          fontFamily:
            "Vazirmatn, IRANSans, Tahoma, Segoe UI, sans-serif",
        },
      }}
    >
      <App />
    </ConfigProvider>
  </React.StrictMode>,
);
