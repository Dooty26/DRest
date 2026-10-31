# Sai lệch cơ sở dữ liệu

## C1 — EF Core Initial migration

- Không có sai lệch nghiệp vụ có chủ ý so với `db/schema.sql`: 25 bảng, CHECK, chỉ mục, kiểu dữ liệu, `xmin` và exclusion constraint được giữ nguyên.
- EF Core thêm bảng vận hành `__EFMigrationsHistory`. Migration cũng bật RLS và thu hồi quyền `anon`/`authenticated` trên bảng này như mọi bảng trong schema `public`.
- Tên constraint/FK/index do EF sinh tuân theo snake_case; một số tên khóa ngoại không được khai báo tường minh trong SQL tham chiếu nhưng không thay đổi hành vi.
- `occupied_until` tiếp tục do ứng dụng tính khi ghi, đúng với schema tham chiếu; không dùng generated column.
- Extension `btree_gist` được cài trong schema `extensions`, còn các bảng ứng dụng nằm trong `public`.
