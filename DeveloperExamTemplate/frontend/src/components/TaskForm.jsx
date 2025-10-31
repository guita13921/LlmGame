import React, { useState } from 'react';

// ฟอร์มนี้ใช้เก็บข้อมูลพื้นฐานของงานใหม่และแจ้งกลับให้คอมโพเนนต์หลัก
const TaskForm = ({ onCreateTask, isSubmitting }) => {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');

  const handleSubmit = (event) => {
    event.preventDefault();
    // ส่งข้อมูลขึ้นไปยัง App เพื่อเรียกใช้งาน API
    onCreateTask({ title, description });
    setTitle('');
    setDescription('');
  };

  return (
    <form onSubmit={handleSubmit}>
      <h2>สร้างงานใหม่</h2>
      <label>
        หัวข้องาน
        <input
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          placeholder="เช่น ออกแบบหน้า Dashboard"
          required
        />
      </label>

      <label>
        รายละเอียด
        <textarea
          value={description}
          onChange={(event) => setDescription(event.target.value)}
          placeholder="อธิบายรายละเอียดงานที่จะทำ"
          rows={4}
          required
        />
      </label>

      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'กำลังบันทึก...' : 'เพิ่มงาน'}
      </button>
    </form>
  );
};

export default TaskForm;
