/*
 * Kyoo - A portable and vast media library solution.
 * Copyright (c) Kyoo.
 *
 * See AUTHORS.md and LICENSE file in the project root for full license information.
 *
 * Kyoo is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * any later version.
 *
 * Kyoo is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Kyoo. If not, see <https://www.gnu.org/licenses/>.
 */

import { View } from "react-native";
import { Image } from "./image";
import { useYoshiki, px, Stylable } from "yoshiki/native";
import { Icon } from "./icons";
import { Skeleton } from "./skeleton";

export const Avatar = ({
	src,
	alt,
	size = px(24),
	isLoading = false,
	...props
}: {
	src?: string | null;
	alt?: string;
	size?: number;
	isLoading?: boolean;
} & Stylable) => {
	const { css } = useYoshiki();

	if (isLoading) return <Skeleton variant="round" {...css({ width: size, height: size })} />;
	return (
		<View {...css({ borderRadius: size / 2, width: size, height: size }, props)}>
			{src ? (
				<Image src={src} alt={alt} layout={{ width: size, height: size }} />
			) : (
				<Icon icon="account-circle" size={size} />
			)}
		</View>
	);
};
