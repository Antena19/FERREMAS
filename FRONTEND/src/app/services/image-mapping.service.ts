import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ImageMappingService {

  // Mapeo de nombres de BD a archivos físicos
  private imageMapping: { [key: string]: string } = {
    'martillo.jpg': 'Martillo_Stanley.webp',
    'destornilladores.jpg': 'Juego_de_Destornilladores.avif',
    'llaves.jpg': 'placeholder.png', // No existe en assets
    'taladro_bosch.jpg': 'Taladro_Percutor_Bosch.webp',
    'sierra_makita.jpg': 'Sierra_Circular_Makita.jpg',
    'lijadora_dewalt.jpg': 'Lijadora_Orbital_DeWalt.jpg',
    'cemento.jpg': 'Cemento_Portland_25kg.jpg',
    'arena.jpg': 'Arena_Fina_40kg.webp',
    'ladrillos.jpg': 'Ladrillos_Prensados.jpg',
    'pintura.jpg': 'Pintura_Látex_Blanco_1_Galón.webp',
    'barniz.jpg': 'Barniz_Marino_1L.png',
    'ceramica.jpg': 'Cerámica_Blanca_30x30cm.webp',
    'casco.jpg': 'Casco_de_Seguridad.webp',
    'guantes.jpg': 'Guantes_de_Trabajo.webp',
    'lentes.jpg': 'Lentes_de_Seguridad.webp'
  };

  /**
   * Obtiene la ruta correcta de la imagen
   * @param dbImageName Nombre de la imagen en la base de datos
   * @returns Ruta completa de la imagen
   */
  getImagePath(dbImageName: string): string {
    const physicalFileName = this.imageMapping[dbImageName] || 'placeholder.png';
    return `assets/img/${physicalFileName}`;
  }

  /**
   * Verifica si existe el mapeo para una imagen
   * @param dbImageName Nombre de la imagen en la base de datos
   * @returns true si existe mapeo, false si no
   */
  hasMapping(dbImageName: string): boolean {
    return this.imageMapping.hasOwnProperty(dbImageName);
  }

  /**
   * Obtiene todos los mapeos disponibles
   * @returns Objeto con todos los mapeos
   */
  getAllMappings(): { [key: string]: string } {
    return { ...this.imageMapping };
  }
} 