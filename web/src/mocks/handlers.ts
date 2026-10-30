import { http, HttpResponse } from 'msw';

export const handlers = [
  // 1. Cấu hình công khai nhà hàng
  http.get('*/api/v1/public/config', () => {
    return HttpResponse.json({
      restaurantName: 'Nhà hàng Dooty Culinary',
      logoUrl: null, // Test logo placeholder
      depositAmount: 200000,
      stayMinutes: 90,
      cleaningMinutes: 15,
      slotStepMinutes: 30,
      depositHoldMinutes: 15,
      fullRefundHoursBefore: 2,
      allergyOptions: [
        'Hải sản',
        'Đậu phộng',
        'Sữa / Lactose',
        'Trứng',
        'Gluten',
        'Đậu nành',
      ],
      viewTypes: [
        { viewType: 'STANDARD', label: 'Tiêu chuẩn', description: 'Không gian ấm cúng, phù hợp gia đình' },
        { viewType: 'WINDOW', label: 'Cửa sổ', description: 'View ngắm phố thơ mộng (Bàn 201)' },
        { viewType: 'BALCONY', label: 'Ban công', description: 'Không gian thoáng mát ngoài trời' },
        { viewType: 'VIP', label: 'Phòng VIP', description: 'Không gian riêng tư, sang trọng' },
        { viewType: 'OUTDOOR', label: 'Sân vườn', description: 'Khu vực gần gũi thiên nhiên' },
      ],
    });
  }),

  // 2. Lưới giờ khả dụng (Tuân thủ Buffer Rule: Bàn 201 có lịch 19:00 -> khóa 17:30...20:30 BOOKED cho WINDOW)
  http.get('*/api/v1/public/availability', ({ request }) => {
    const url = new URL(request.url);
    const date = url.searchParams.get('date') || new Date().toISOString().split('T')[0];

    const times = [
      '11:00', '11:30', '12:00', '12:30', '13:00', '13:30',
      '17:00', '17:30', '18:00', '18:30', '19:00', '19:30', '20:00', '20:30', '21:00'
    ];

    const slots = times.map((t) => {
      // Đối với WINDOW: 17:30 - 20:30 bị khóa BOOKED do Bàn 201 có lịch 19:00 (|t - 19:00| <= 105 phút)
      const isWindowBlocked = ['17:30', '18:00', '18:30', '19:00', '19:30', '20:00', '20:30'].includes(t);

      return {
        time: t,
        viewTypes: [
          {
            viewType: 'STANDARD',
            isAvailable: true,
            availableTables: 3,
          },
          {
            viewType: 'WINDOW',
            isAvailable: !isWindowBlocked,
            availableTables: isWindowBlocked ? 0 : 1,
            reason: isWindowBlocked ? 'BOOKED' : undefined,
          },
          {
            viewType: 'BALCONY',
            isAvailable: true,
            availableTables: 2,
          },
          {
            viewType: 'VIP',
            isAvailable: t !== '12:00',
            availableTables: t === '12:00' ? 0 : 1,
            reason: t === '12:00' ? 'BOOKED' : undefined,
          },
        ],
      };
    });

    return HttpResponse.json({
      date,
      slots,
    });
  }),

  // 3. Tạo đặt bàn công khai
  http.post('*/api/v1/public/reservations', async ({ request }) => {
    const body = (await request.json()) as any;
    const now = new Date();
    const expiresAt = new Date(now.getTime() + 15 * 60 * 1000).toISOString();

    return HttpResponse.json(
      {
        code: 'RES008899',
        status: 'PENDING_DEPOSIT',
        table: {
          id: 104,
          name: 'Bàn 104',
          viewType: body.viewType || 'STANDARD',
          areaName: 'Tầng 1',
        },
        startsAt: `${body.date || '2026-10-31'}T${body.time || '19:00'}:00Z`,
        partySize: body.partySize || 4,
        deposit: {
          amount: 200000,
          expiresAt,
          transferContent: 'RES008899 0987654321',
          qrPayload: '00020101021238540010A000000727012600069704220112RES00889952040000530370454062000005802VN62250821RES008899 09876543216304E1F2',
          bank: {
            bankName: 'VietinBank',
            accountNo: '109876543210',
            accountName: 'NHA HANG DOOTY',
          },
        },
      },
      { status: 201 }
    );
  }),

  // 4. Tra cứu đặt bàn theo mã code
  http.get('*/api/v1/public/reservations/:code', ({ params }) => {
    const { code } = params;
    return HttpResponse.json({
      code: code as string,
      status: 'CONFIRMED',
      depositStatus: 'PAID',
      startsAt: '2026-10-31T12:00:00Z',
      partySize: 4,
      table: {
        id: 104,
        name: 'Bàn 104',
        viewType: 'STANDARD',
        areaName: 'Tầng 1',
      },
      canCancel: true,
      refundPreview: {
        eligible: true,
        mode: ['REFUND', 'VOUCHER'],
        amount: 200000,
      },
      wishlist: {
        enabled: true,
        targetViewType: 'WINDOW',
      },
    });
  }),

  // 5. Thực đơn công khai
  http.get('*/api/v1/public/menu', () => {
    return HttpResponse.json({
      categories: [
        {
          id: 1,
          name: 'Món khai vị',
          items: [
            {
              id: 101,
              name: 'Salad bò sốt chanh leo',
              price: 125000,
              imageUrl: 'https://images.unsplash.com/photo-1540420773420-3366772f4999?w=500',
              description: 'Xà lách tươi giòn, thăn bò áp chảo mềm ngọt cùng sốt chanh leo chua thanh',
              isAvailable: true,
            },
            {
              id: 102,
              name: 'Súp bí đỏ kem tươi',
              price: 85000,
              imageUrl: null, // Thử nghiệm placeholder khi null
              description: 'Bí đỏ hồ lô ninh nhuyễn béo ngậy vị bơ và kem tươi whipping Pháp',
              isAvailable: true,
            },
          ],
        },
        {
          id: 2,
          name: 'Món chính',
          items: [
            {
              id: 201,
              name: 'Bò bít tết Wagyu sốt tiêu đen',
              price: 350000,
              imageUrl: 'https://images.unsplash.com/photo-1544025162-d76694265947?w=500',
              description: 'Thăn lưng bò Wagyu A4 nướng than hồng, kèm khoai tây nghiền và măng tây',
              isAvailable: true,
            },
            {
              id: 202,
              name: 'Cá hồi Na Uy sốt cam thì là',
              price: 285000,
              imageUrl: 'https://images.unsplash.com/photo-1519708227418-c8fd9a32b7a2?w=500',
              description: 'Cá hồi áp chảo da giòn, thịt mọng nước kèm sốt bơ cam thơm lừng',
              isAvailable: false, // Thử nghiệm món tạm hết
            },
          ],
        },
      ],
    });
  }),

  // 6. Phiên bàn QR
  http.get('*/api/v1/public/tables/:tableCode/session', ({ params }) => {
    const { tableCode } = params;
    return HttpResponse.json({
      tableName: `Bàn ${tableCode}`,
      areaName: 'Tầng 1',
      isOpen: true,
      orderId: 901,
    });
  }),

  // 7. Gửi món QR
  http.post('*/api/v1/public/tables/:tableCode/orders', () => {
    return HttpResponse.json({
      orderId: 901,
      items: [
        { detailId: 1001, status: 'NEW' },
        { detailId: 1002, status: 'NEW' },
      ],
    });
  }),
];
