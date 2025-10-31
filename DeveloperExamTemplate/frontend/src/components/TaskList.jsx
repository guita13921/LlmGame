import React from 'react';

// คอมโพเนนต์รับรายการงานทั้งหมดและแสดงผลเป็นลิสต์อย่างเรียบง่าย
const TaskList = ({ tasks }) => {
  if (!tasks.length) {
    // เมื่อไม่มีงานให้แสดงข้อความอธิบายอย่างชัดเจน
    return <p>ยังไม่มีงานที่ต้องทำ ลองเพิ่มงานใหม่ดูนะ!</p>;
  }

  return (
    <section>
      <h2>รายการงาน</h2>
      <ul>
        {tasks.map((task) => (
          // ใช้ค่า id ที่ได้จากฐานข้อมูลเป็น key เพื่อช่วย React จัดการ DOM
          <li key={task.id}>
            <h3>{task.title}</h3>
            <p>{task.description}</p>
            <span className="status">สถานะ: {task.status}</span>
          </li>
        ))}
      </ul>
    </section>
  );
};

export default TaskList;
