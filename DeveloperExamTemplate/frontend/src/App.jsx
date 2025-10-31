import React, { useEffect, useState } from 'react';
import axios from 'axios';
import TaskList from './components/TaskList.jsx';
import TaskForm from './components/TaskForm.jsx';

// The API base URL is isolated so candidates can change it for different environments.
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:4000/api';

const App = () => {
  // Local state keeps the tasks retrieved from the backend API.
  const [tasks, setTasks] = useState([]);
  // Track whether the template is currently loading data from the backend.
  const [isLoading, setIsLoading] = useState(false);
  // Store any error messages to help candidates debug integration problems quickly.
  const [error, setError] = useState(null);

  // Fetch tasks from the backend when the component mounts.
  useEffect(() => {
    const fetchTasks = async () => {
      setIsLoading(true);
      setError(null);
      try {
        const response = await axios.get(`${API_BASE_URL}/tasks`);
        setTasks(response.data);
      } catch (err) {
        // Expose only the relevant information so that candidates can display it in the UI.
        setError(err.response?.data?.message ?? 'ไม่สามารถเชื่อมต่อ API ได้');
      } finally {
        setIsLoading(false);
      }
    };

    fetchTasks();
  }, []);

  // Handle task creation by delegating to the backend API.
  const handleCreateTask = async (taskData) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await axios.post(`${API_BASE_URL}/tasks`, taskData);
      setTasks((current) => [...current, response.data]);
    } catch (err) {
      setError(err.response?.data?.message ?? 'ไม่สามารถสร้างงานใหม่ได้');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="app-container">
      <header>
        <h1>Developer Exam Front-end Template</h1>
        <p>
          ใช้โครงร่างนี้เพื่อแสดงทักษะด้าน Front-end โดยการเชื่อมต่อกับ Back-end,
          จัดการสถานะ และออกแบบ UI
        </p>
      </header>

      {/* TaskForm จะรับผิดชอบการสร้างงานใหม่และแจ้งผลกลับมาที่คอมโพเนนต์หลัก */}
      <TaskForm onCreateTask={handleCreateTask} isSubmitting={isLoading} />

      {/* แสดงสถานะการโหลดและข้อผิดพลาดเพื่อสื่อสารกับผู้ใช้ได้ชัดเจน */}
      {isLoading && <p>กำลังโหลดข้อมูล...</p>}
      {error && <p className="error">{error}</p>}

      {/* ส่งรายการงานไปยังคอมโพเนนต์ย่อยเพื่อการแยกความรับผิดชอบ */}
      <TaskList tasks={tasks} />
    </div>
  );
};

export default App;
