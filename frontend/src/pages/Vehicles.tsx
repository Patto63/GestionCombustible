import { useEffect, useState } from 'react';
import { apiFetch } from '../api/base';

interface VehiculoDto {
  vehiculoId: number;
  codigo: string;
  placa: string;
  descripcion: string;
}

export const Vehicles = () => {
  const [items, setItems] = useState<VehiculoDto[]>([]);

  useEffect(() => {
    apiFetch<{ vehiculos: VehiculoDto[] }>('vehicle')
      .then(r => setItems(r.vehiculos))
      .catch(err => console.error(err));
  }, []);

  return (
    <div>
      <h2>Vehículos</h2>
      <ul>
        {items.map(v => (
          <li key={v.vehiculoId}>{v.codigo} - {v.placa}</li>
        ))}
      </ul>
    </div>
  );
};
